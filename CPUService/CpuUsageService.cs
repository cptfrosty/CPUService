using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

// -------------------------------------------------------------------
// Сервис обновляет данные о нагрузки процессора каждую 0.1 сек.
// Отправка данных на сервер каждые 0.1 сек
//  - (при условии есть запущено серверное приложение)
// -------------------------------------------------------------------

namespace CPUService
{
    public partial class CpuUsageService : ServiceBase
    {
        private Thread listenerThread;
        private Thread updateThread;
        private int port = 8004; //  Новый порт, чтобы не конфликтовать
        private PerformanceCounter cpuCounter;
        private volatile string currentCpuUsage = "0.00";  //  Потокобезопасное хранение данных
        private CancellationTokenSource cancellationTokenSource;  //  Для отмены потоков

        public CpuUsageService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            cancellationTokenSource = new CancellationTokenSource(); // Create token source

            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // "Прогреваем" счетчик

                // Запускаем поток для периодического обновления нагрузки CPU
                updateThread = new Thread(() => UpdateCpuUsage(cancellationTokenSource.Token));
                updateThread.IsBackground = true;
                updateThread.Start();

                // Запускаем поток, который будет слушать подключения от сервера
                listenerThread = new Thread(() => StartListeningAsync(cancellationTokenSource.Token));
                listenerThread.IsBackground = true; // Демон-поток
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске службы: {ex.Message}");
                Stop();
            }
        }

        protected override void OnStop()
        {
            // Останавливаем потоки
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();

                if (updateThread != null && updateThread.IsAlive)
                {
                    updateThread.Join(5000);
                }
                if (listenerThread != null && listenerThread.IsAlive)
                {
                    listenerThread.Join(5000);
                }
                // Освобождение ресурсов
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null; // null для предотвращения случайного повторного использования
            }

            // Освобождение ресурсов
            if (cpuCounter != null)
            {
                cpuCounter.Dispose();
                cpuCounter = null; // null для предотвращения случайного повторного использования
            }
        }

        private void UpdateCpuUsage(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        float cpuValue = GetCpuUsage();
                        currentCpuUsage = cpuValue.ToString("F2"); // Обновляем текущее значение
                        Thread.Sleep(100); // Обновляем каждую 0.1 секунду
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort(); // Отменяет прерывание потока
                        break;
                    }
                    catch (Exception ex)
                    {
                        //  Если ошибка критическая, то можно завершить поток (return;)
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
            finally
            {
            }
        }

        private async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Используется AcceptTcpClientAsync, чтобы избежать блокировки
                        Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
                        await Task.WhenAny(acceptTask, Task.Delay(50, cancellationToken));

                        if (acceptTask.IsCompleted)
                        {
                            TcpClient client = await acceptTask;

                            ThreadPool.QueueUserWorkItem(HandleClientThread, new Tuple<TcpClient, CancellationToken>(client, cancellationToken));
                        }
                        else if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                    catch (SocketException ex)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                listener?.Stop();
            }
        }


        private async void HandleClientThread(object state)
        {
            var tuple = (Tuple<TcpClient, CancellationToken>)state;
            TcpClient client = tuple.Item1;
            CancellationToken cancellationToken = tuple.Item2;

            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(stream)) // Добавили reader, вдруг клиент что-то отправит
                {
                    while (client.Connected && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            string cpuUsage = GetCpuUsageSafe(); // Получить CPU
                            await writer.WriteLineAsync(cpuUsage); // Отправляем *текущее* значение
                            await Task.Delay(100, cancellationToken); // Отправляем данные каждые 0.1 сек.
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (IOException ex)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                client.Close();
            }
        }

        private async Task<string> ReadLineAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            try
            {
                Task<string> readLineTask = reader.ReadLineAsync();
                await Task.WhenAny(readLineTask, Task.Delay(Timeout.Infinite, cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                else if (readLineTask.IsCompleted)
                {
                    return await readLineTask;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private string GetCpuUsageSafe()
        {
            try
            {
                return cpuCounter.NextValue().ToString("F2");
            }
            catch (Exception ex)
            {
                return "0.00";
            }
        }

        private float GetCpuUsage()
        {
            try
            {
                return cpuCounter.NextValue();
            }
            catch (InvalidOperationException ex)
            {
                // Возможно, счетчик не был инициализирован или не существует
                // Попробуйте пересоздать счетчик или вернуть значение по умолчанию
                return 0;
            }
            catch (Win32Exception ex)
            {
                // Возможно, проблема с правами доступа
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}

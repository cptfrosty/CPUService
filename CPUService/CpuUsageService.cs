using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
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
                // Handle the error (e.g., stop the service, log the error)
                Stop(); // Stop the service if it fails to start
            }
        }

        protected override void OnStop()
        {
            Console.WriteLine("OnStop: Starting to stop service...");
            // Останавливаем потоки
            if (cancellationTokenSource != null)
            {
                Console.WriteLine("OnStop: Requesting cancellation...");
                cancellationTokenSource.Cancel(); // Request cancellation

                // Optional: Wait for the threads to stop (with a timeout)
                if (updateThread != null && updateThread.IsAlive)
                {
                    Console.WriteLine("OnStop: Waiting for updateThread to finish...");
                    updateThread.Join(5000); // Wait 5 seconds
                }
                if (listenerThread != null && listenerThread.IsAlive)
                {
                    Console.WriteLine("OnStop: Waiting for listenerThread to finish...");
                    listenerThread.Join(5000); // Wait 5 seconds
                }
                // Dispose of the CancellationTokenSource to release resources
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null; // Set to null to prevent accidental reuse
                Console.WriteLine("OnStop: CancellationTokenSource disposed.");
            }

            // Release resources
            if (cpuCounter != null)
            {
                Console.WriteLine("OnStop: Disposing PerformanceCounter...");
                cpuCounter.Dispose();
                cpuCounter = null; // Set to null to prevent accidental reuse
                Console.WriteLine("OnStop: PerformanceCounter disposed.");
            }
            Console.WriteLine("OnStop: Service stopped.");
        }

        private void UpdateCpuUsage(CancellationToken cancellationToken)
        {
            Console.WriteLine("UpdateCpuUsage: Thread started.");
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
                        // Handle ThreadAbortException (if you *really* need to, but usually not)
                        Console.WriteLine("UpdateCpuUsage: ThreadAbortException caught.");
                        Thread.ResetAbort(); // Clears the thread abort
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UpdateCpuUsage: Ошибка: {ex.Message}");
                        //  Если ошибка критическая, то можно завершить поток (return;)
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("UpdateCpuUsage: ThreadInterruptedException caught.");
            }
            finally
            {
                Console.WriteLine("UpdateCpuUsage: Thread finished.");
            }
        }

        private async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StartListening: Thread started.");
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"StartListening: Listening on port {port}...");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Use PendingAsync to avoid blocking
                        Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
                        await Task.WhenAny(acceptTask, Task.Delay(50, cancellationToken));

                        if (acceptTask.IsCompleted)
                        {
                            TcpClient client = await acceptTask;
                            Console.WriteLine("StartListening: Accepted connection.");

                            // Start client handling in separate thread
                            ThreadPool.QueueUserWorkItem(HandleClientThread, new Tuple<TcpClient, CancellationToken>(client, cancellationToken));
                        }
                        else if (cancellationToken.IsCancellationRequested)
                        {
                            Console.WriteLine("StartListening: Cancellation requested, exiting.");
                            break;
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"StartListening: SocketException: {ex.Message}");
                        // Consider restarting the listener or logging the error and exiting
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"StartListening: Error: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartListening: Fatal error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("StartListening: Thread is stopping, closing listener...");
                listener?.Stop();
                Console.WriteLine("StartListening: Listener stopped.");
            }
            Console.WriteLine("StartListening: Thread finished.");
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
                            // Check if the client sent any data
                            if (stream.DataAvailable)
                            {
                                string request = await ReadLineAsync(reader, cancellationToken); // read the data from client
                                if (request != null)
                                {
                                    Console.WriteLine($"HandleClient: Received request: {request}");
                                    //You can add logic based on this request here for extra commands
                                }
                            }
                            string cpuUsage = GetCpuUsageSafe(); // Get the CPU usage
                            await writer.WriteLineAsync(cpuUsage); // Отправляем *текущее* значение
                            await Task.Delay(100, cancellationToken); // Отправляем данные каждые 0.1 сек.
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("HandleClient: Task was cancelled.");
                            break;
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine($"HandleClient: IOException: {ex.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"HandleClient: Error: {ex.Message}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HandleClient: Outer Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("HandleClient: Connection closed.");
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
                    Console.WriteLine("ReadLineAsync: Cancellation requested.");
                    return null;
                }
                else if (readLineTask.IsCompleted)
                {
                    return await readLineTask;
                }
                else
                {
                    Console.WriteLine("ReadLineAsync: Timeout occurred.");
                    return null; // or throw an exception if you want to indicate a timeout
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReadLineAsync: Exception: {ex.Message}");
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
                Console.WriteLine($"GetCpuUsageSafe: Error getting CPU usage: {ex.Message}");
                return "0.00"; // Return a default value
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
                Console.WriteLine($"GetCpuUsage: InvalidOperationException: {ex.Message}");
                // Возможно, счетчик не был инициализирован или не существует
                // Попробуйте пересоздать счетчик или вернуть значение по умолчанию
                return 0;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine($"GetCpuUsage: Win32Exception: {ex.Message}");
                // Возможно, проблема с правами доступа
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCpuUsage: Exception: {ex.Message}");
                return 0; // or throw the exception, depending on the desired behavior
            }
        }
    }
}

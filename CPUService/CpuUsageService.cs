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
        private int port = 8004; //  Новый порт, чтобы не конфликтовать
        private PerformanceCounter cpuCounter;
        private volatile string currentCpuUsage = "0.00";  //  Потокобезопасное хранение данных

        public CpuUsageService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // "Прогреваем" счетчик

            // Запускаем поток для периодического обновления нагрузки CPU
            Thread updateThread = new Thread(UpdateCpuUsage);
            updateThread.IsBackground = true;
            updateThread.Start();

            // Запускаем поток, который будет слушать подключения от сервера
            listenerThread = new Thread(StartListening);
            listenerThread.IsBackground = true; // Демон-поток
            listenerThread.Start();
        }

        protected override void OnStop()
        {
            // Останавливаем потоки
            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Abort(); //  Use with caution, but necessary here.
            }
        }

        private void UpdateCpuUsage()
        {
            while (true)
            {
                float cpuValue = GetCpuUsage();
                currentCpuUsage = cpuValue.ToString("F2"); // Обновляем текущее значение
                Thread.Sleep(100); // Обновляем каждую 0.1 секунду
            }
        }

        private void StartListening()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"Сервис запущен. Прослушиваю порт {port}...");

                while (true)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Console.WriteLine("Принято подключение.");

                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.IsBackground = true;
                        clientThread.Start();
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Ошибка принятия подключения: {ex.Message}");
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка при запуске слушателя: {ex.Message}");
            }
            finally
            {
                listener?.Stop();
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(stream)) // Добавили reader, вдруг клиент что-то отправит
                {
                    while (client.Connected)
                    {
                        // Check if the client sent any data
                        if (stream.DataAvailable)
                        {
                            string request = reader.ReadLine(); // read the data from client
                            Console.WriteLine($"Received request: {request}");
                            //You can add logic based on this request here for extra commands
                        }
                        writer.WriteLine(currentCpuUsage); // Отправляем *текущее* значение
                        Thread.Sleep(100); // Отправляем данные каждые 0.1 сек.
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Соединение закрыто.");
            }
        }

        private float GetCpuUsage()
        {
            return cpuCounter.NextValue(); // Возвращаем значение счетчика
        }
    }
}

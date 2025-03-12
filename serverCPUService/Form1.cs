using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace serverCPUService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Task.Run(async () => { await Main(); });
        }

        private static int clientPort = 8005;  //  Порт для прослушивания клиентов
        private static string serviceAddress = "localhost";
        private static int servicePort = 8004;
        private static string lastCpuUsage = "0.00"; // Cache

        static async Task Main()
        {
            TcpListener clientListener = null;

            try
            {
                // Connect to the service (runs in a separate thread)
                Task task = Task.Run(() => ConnectToService());

                clientListener = new TcpListener(IPAddress.Any, clientPort);
                clientListener.Start();

                Console.WriteLine($"Сервер запущен. Прослушивает клиентов на порту {clientPort}...");

                while (true)
                {
                    TcpClient client = await clientListener.AcceptTcpClientAsync();
                    Console.WriteLine("Принято подключение от клиента.");

                    await HandleClient(client);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка при запуске слушателя клиентов: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                clientListener?.Stop();
            }
        }

        private static async Task ConnectToService()
        {
            while (true)
            {
                try
                {
                    using (TcpClient serviceClient = new TcpClient())
                    {
                        await serviceClient.ConnectAsync(serviceAddress, servicePort);
                        Console.WriteLine("Подключено к службе.");

                        using (NetworkStream serviceStream = serviceClient.GetStream())
                        using (StreamReader serviceReader = new StreamReader(serviceStream))
                        {
                            while (serviceClient.Connected)
                            {
                                string cpuUsage = await serviceReader.ReadLineAsync();
                                if (cpuUsage != null)
                                {
                                    lastCpuUsage = cpuUsage; // Update the cache
                                }
                                else
                                {
                                    Console.WriteLine("Служба отключилась.");
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Ошибка при подключении к службе: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                Console.WriteLine("Повторное подключение к службе через 5 секунд...");
                await Task.Delay(5000);
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream clientStream = client.GetStream())
                using (StreamReader clientReader = new StreamReader(clientStream))
                using (StreamWriter clientWriter = new StreamWriter(clientStream) { AutoFlush = true })
                {
                    while (client.Connected)
                    {
                        string clientCommand = await clientReader.ReadLineAsync();
                        if (clientCommand == null)
                        {
                            Console.WriteLine("Клиент отключился.");
                            break;
                        }

                        Console.WriteLine($"Получена команда от клиента: {clientCommand}");

                        string response = ProcessCommand(clientCommand); // Process the command
                        await clientWriter.WriteLineAsync(response);
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Соединение с клиентом закрыто.");
            }
        }

        private static string ProcessCommand(string command)
        {
            switch (command.ToLower())
            {
                case "getcpu":
                    return lastCpuUsage; // Use the cached value
                case "hello":
                    return "Hello from the server!";
                default:
                    return "Неизвестная команда";
            }
        }
    }
}

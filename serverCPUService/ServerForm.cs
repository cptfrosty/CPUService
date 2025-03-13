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
using System.Configuration;

namespace serverCPUService
{
    public partial class ServerForm : Form
    {
        private static int clientPort = 8005;  //  Порт для прослушивания клиентов
        private static string serviceAddress = "localhost";
        private static int servicePort = 8004;
        //private static string lastCpuUsage = "0.00"; // Cache

        private static AppLauncher _appLauncher = null;

        public ServerForm()
        {
            InitializeComponent();
            this.Shown += ServerForm_Shown;
        }

        private async void ServerForm_Shown(object sender, EventArgs e)
        {
            LogConsole.Init();
            VisualCheckService();
            await Main();
        }

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
                                    CpuCounter.Add(cpuUsage);
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
            string[] commands = command.Split(';');

            switch (commands[0].ToLower())
            {
                case "getcpu":
                    return CpuCounter.GetAvarage().ToString("0.00");
                case "app":
                    if(_appLauncher != null)
                    {
                        _appLauncher.StopMonitoring();
                    }

                    //----------------------------------------
                    //Струтура: (0) - команда
                    //          (1) - путь до файла
                    //          (2) - сколько должно работать
                    //          (3) - процент загружености
                    //----------------------------------------

                    _appLauncher = new AppLauncher(int.Parse(commands[2]), int.Parse(commands[3]));
                    string result = _appLauncher.SetProgramPath(commands[1]);
                    if (result == "Успешно") 
                         Task.Run(()=>_appLauncher.StartMonitoringAsync());

                    return result;
                default:
                    return "Неизвестная команда";
            }
        }

        /// <summary>
        /// Проверить статус службы
        /// </summary>
        private void VisualCheckService()
        {
            var status = ServiceManager.GetServiceStatus(ConfigurationManager.AppSettings["NameService"]);

            switch (status)
            {
                case null:
                    btnOnService.Enabled = false;
                    btnOffService.Enabled = false;
                    btnReloadService.Enabled = false;

                    labelInfo.Text = "Служба не найдена";
                    break;
                case System.ServiceProcess.ServiceControllerStatus.Stopped:
                case System.ServiceProcess.ServiceControllerStatus.StopPending:
                    btnOnService.Enabled = true;
                    btnOffService.Enabled = false;
                    btnReloadService.Enabled = false;
                    break;
                case System.ServiceProcess.ServiceControllerStatus.Running:
                case System.ServiceProcess.ServiceControllerStatus.StartPending:
                    btnOnService.Enabled = false;
                    btnOffService.Enabled = true;
                    btnReloadService.Enabled = true;
                    break;
            }
        }

        //TODO: пусть методы работают асинхронно и после выполнения, будет срабатывать VisualCheckService
        // Так же VisualCheckService должен предварительно отключать некоторые кнопки, пока выполняется действие асихнронно
        private async void btnOnService_Click(object sender, EventArgs e)
        {
            await ServiceManager.StartService(ConfigurationManager.AppSettings["NameService"]);
            VisualCheckService();
        }

        private void btnOffService_Click(object sender, EventArgs e)
        {
            ServiceManager.StopService(ConfigurationManager.AppSettings["NameService"]);
            VisualCheckService();
        }
    }
}

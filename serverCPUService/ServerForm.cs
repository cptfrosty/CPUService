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
using System.ServiceProcess;

namespace serverCPUService
{
    public partial class ServerForm : Form
    {
        private static int clientPort = 8005;  //  Порт для прослушивания клиентов
        private static string serviceAddress = "localhost";
        private static int servicePort = 8004;
        //private static string lastCpuUsage = "0.00"; // Cache

        private static AppLauncher _appLauncher = null;
        private static ServiceMonitor _serviceMonitor;

        public ServerForm()
        {
            InitializeComponent();
            this.Shown += ServerForm_Shown;
        }

        private async void ServerForm_Shown(object sender, EventArgs e)
        {
            LogConsole.Init();
            //VisualCheckService();

            _serviceMonitor = new ServiceMonitor(ConfigurationManager.AppSettings["NameService"], 1000);
            Task task = Task.Run(() => _serviceMonitor.StartMonitoringAsync());
            _serviceMonitor.ServiceStatusChanged += _serviceMonitor_ServiceStatusChanged;

            UpdateUI( await ServiceManager.GetServiceStatus(ConfigurationManager.AppSettings["NameService"]));

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


        private void _serviceMonitor_ServiceStatusChanged(object sender, ServiceMonitor.ServiceStatusChangedEventArgs e)
        {
            UpdateUI(e.NewStatus);
        }

        /// <summary>
        /// Обновляет пользовательский интерфейс
        /// </summary>
        /// <param name="status">Статус: null - отключить все кнопки</param>
        private void UpdateUI(ServiceControllerStatus? status)
        {
            // Проверяем, требуется ли переключение потока (Invoke)
            if (this.InvokeRequired)
            {
                // Переключаемся в главный поток и выполняем обновление UI
                this.Invoke(new Action(() => UpdateUI(status)));
                return; // Прекратить выполнение текущего вызова, чтобы избежать повторного обновления
            }

            // Обновление UI в главном потоке
            if (status == null)
            {
                btnOnService.Enabled = false;
                btnOffService.Enabled = false;
                btnReloadService.Enabled = false;
            }
            else
            {
                switch (status)
                {
                    case ServiceControllerStatus.Stopped:
                    case ServiceControllerStatus.StopPending:
                        btnOnService.Enabled = true;
                        btnOffService.Enabled = false;
                        btnReloadService.Enabled = false;
                        labelInfo.Text = "Служба остановлена"; // Optional: Set descriptive text.
                        break;
                    case ServiceControllerStatus.Running:
                    case ServiceControllerStatus.StartPending:
                        btnOnService.Enabled = false;
                        btnOffService.Enabled = true;
                        btnReloadService.Enabled = true;
                        labelInfo.Text = "Служба запущена"; // Optional: Set descriptive text.
                        break;
                    default:
                        btnOnService.Enabled = false;
                        btnOffService.Enabled = false;
                        btnReloadService.Enabled = false;
                        labelInfo.Text = "Неизвестный статус службы";
                        break;
                }
            }
        }

        private async void btnOnService_Click(object sender, EventArgs e)
        {
            await ServiceManager.StartService(ConfigurationManager.AppSettings["NameService"]);
            UpdateUI(null);

            //VisualCheckService();
        }

        private async void btnOffService_Click(object sender, EventArgs e)
        {
            await ServiceManager.StopService(ConfigurationManager.AppSettings["NameService"]);
            UpdateUI(null);
            //VisualCheckService();
        }
    }
}

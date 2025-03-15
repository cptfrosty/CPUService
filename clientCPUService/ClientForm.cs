using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

//------------------------------------------------------------------
//  Отличие значения в приложение от показателей в диспетчере задач:
// В диспетчере задач сбор осуществляется тоже через средний показатель.
// С определённой частотой он опрашивает процессор и вычисляет среднне,
// с какой частотой и сколько средних значений он принимает неизвестно,
// но эти показатели можно настроить в:
// 1) В самом сервисе
// 2) На сервере
// 3) На клиентском приложении
//------------------------------------------------------------------


namespace clientCPUService
{
    public partial class ClientForm : Form
    {
        private int serverPort = 8005;  // Подключаемся к Server Application!
        private string serverAddress = "localhost";
        
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        
        private bool isConnected = false;
        private System.Threading.Timer timer; // Таймер для опроса

        public ClientForm()
        {
            InitializeComponent();
            Task.Run(() => { ConnectToServer(); });
        }

        private async void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);

                NetworkStream stream = client.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                isConnected = true;

                this.Invoke((MethodInvoker)delegate
                {
                    VisualStatusConnectToServer(isConnected);
                });
                //Подключено к серверу

                // Запускаем таймер для опроса
                timer = new System.Threading.Timer(GetCpuUsage, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.1f));
            }
            catch (SocketException ex)
            {
                //Ошибка подключения
                this.Invoke((MethodInvoker)delegate
                {
                    VisualStatusConnectToServer(isConnected);
                });
            }
        }

        /// <summary>
        /// Получение значения CPU с сервера
        /// </summary>
        /// <param name="state"></param>
        private async void GetCpuUsage(object state = null)
        {
            if (!isConnected) return;  // Выходим, если не подключены

            try
            {
                await writer.WriteLineAsync("getcpu");

                string response = await reader.ReadLineAsync();

                if (response != null)
                {
                    // Обновляем UI (безопасно для потока)
                    this.Invoke((MethodInvoker)delegate {
                        UpdateVisualInfoCpu(response);
                    });
                }
                else
                {
                    // Обрабатываем разрыв соединения (безопасно для потока)
                    this.Invoke((MethodInvoker)delegate {
                        //Соединение прервано
                        Disconnect();
                    });
                }
            }
            catch (IOException ex)
            {
                // Обрабатываем ошибку (безопасно для потока)
                this.Invoke((MethodInvoker)delegate {
                    //Ошибка при обмене данными
                    Disconnect();
                });
            }
            catch (Exception ex)
            {
                //Обрабатываем ошибку (безопасно для потока)
                this.Invoke((MethodInvoker)delegate {
                    //Произошла непредвиденная ошибка
                    Disconnect();
                });
            }
        }

        private async void SetServerSettings(Settings settings)
        {
            if (!isConnected) return;  // Выходим, если не подключены

            string command = "app";
            int seconds = settings.GetSeconds();
            string path = settings.Path;
            int maxLoad = settings.MaximumProcessorLoad;

            string request = $"{command};{path};{seconds};{maxLoad}";

            try
            {
                await writer.WriteLineAsync(request);

                string response = await reader.ReadLineAsync();

                if (response != null)
                {
                    // Обновляем UI (безопасно для потока)
                    this.Invoke((MethodInvoker)delegate {
                        MessageBox.Show(response);
                    });
                }
                else
                {
                    // Обрабатываем разрыв соединения (безопасно для потока)
                    this.Invoke((MethodInvoker)delegate {
                        Disconnect();
                    });
                }
            }
            catch (IOException ex)
            {
                // Обрабатываем ошибку (безопасно для потока)
                this.Invoke((MethodInvoker)delegate {
                    //Ошибка при обмене данными
                    Disconnect();
                });
            }
            catch (Exception ex)
            {
                //Обрабатываем ошибку (безопасно для потока)
                this.Invoke((MethodInvoker)delegate {
                    //Произошла непредвиденная ошибка
                    Disconnect();
                });
            }
        }

        private void Disconnect()
        {
            if (isConnected)
            {
                try
                {
                    // Останавливаем таймер
                    timer?.Dispose();
                    timer = null;

                    writer?.Close();
                    reader?.Close();
                    client?.Close();
                }
                catch (Exception ex)
                {
                    //Ошибка при отключении
                }
                finally
                {
                    isConnected = false;
                    VisualStatusConnectToServer(isConnected);
                    //Отключено от сервера.");
                }
            }
        }

        /// <summary>
        /// Визуальное отображение статуса соединения
        /// </summary>
        private void VisualStatusConnectToServer(bool isConnect)
        {
            if (isConnect)
            {
                labelStatusConnect.Text = "Доступен";
                labelStatusConnect.ForeColor = Color.Green;
            }
            else
            {
                labelStatusConnect.Text = "Не доступен";
                labelStatusConnect.ForeColor = Color.Red;
            }
        }

        private void UpdateVisualInfoCpu(string response)
        {
            labelCPUInfo.Text = $"{response}%";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Hour = int.Parse(numericUpDownHour.Value.ToString());
            settings.Minutes = int.Parse(numericUpDownMinutes.Value.ToString());
            settings.Path = tbLaunchApp.Text;
            settings.MaximumProcessorLoad = int.Parse(numericUpDownMaxAvCpu.Value.ToString());

            Task.Run(() => { SetServerSettings(settings); });
        }
    }
}

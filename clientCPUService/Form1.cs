using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace clientCPUService
{
    public partial class Form1 : Form
    {
        private string serverAddress = "localhost";
        private int serverPort = 8005;  // Подключаемся к Server Application!
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private bool isConnected = false;
        private System.Threading.Timer timer; // Таймер для опроса

        public Form1()
        {
            InitializeComponent();
            Task.Run(() => { ConnectButton_Click(); });
        }

        private async void ConnectButton_Click()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);

                NetworkStream stream = client.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                isConnected = true;
                UpdateButtonStates();
                AppendToLog("Подключено к серверу.");

                // Запускаем таймер для опроса
                timer = new System.Threading.Timer(GetCpuUsage, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.1f));
            }
            catch (SocketException ex)
            {
                AppendToLog("Ошибка подключения: " + ex.Message);
            }
        }

        private async void GetCpuUsage(object state)  // <- Изменили тип
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
                        labelCPUInfo.Text = $"{response}%";
                    });
                }
                else
                {
                    // Обрабатываем разрыв соединения (безопасно для потока)
                    this.Invoke((MethodInvoker)delegate {
                        AppendToLog("Соединение прервано.");
                        Disconnect();
                    });
                }
            }
            catch (IOException ex)
            {
                // Обрабатываем ошибку (безопасно для потока)
                this.Invoke((MethodInvoker)delegate {
                    AppendToLog("Ошибка при обмене данными: " + ex.Message);
                    Disconnect();
                });
            }
            catch (Exception ex)
            {
                //Обрабатываем ошибку (безопасно для потока)
                this.Invoke((MethodInvoker)delegate {
                    AppendToLog("Произошла непредвиденная ошибка: " + ex.Message);
                    Disconnect();
                });
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            Disconnect();
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
                    AppendToLog("Ошибка при отключении: " + ex.Message);
                }
                finally
                {
                    isConnected = false;
                    UpdateButtonStates();
                    AppendToLog("Отключено от сервера.");
                }
            }
        }

        private void AppendToLog(string message)
        {
            //logTextBox.AppendText(message + Environment.NewLine);
        }

        private void UpdateButtonStates()
        {
            //ConnectButton.Enabled = !isConnected;
            //GetCpuButton.Enabled = isConnected; // Deprecated
            //DisconnectButton.Enabled = isConnected;
        }

        // Remove
    }
}

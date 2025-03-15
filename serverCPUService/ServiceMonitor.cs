using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

//-------------------------------------------------------------------------------
// Отслеживает состояние службы:
// Пример использования:
//
//      public static async Task StartMonitoring()
//      {
//          1. Укажите название службы
//          string serviceName = "Monitors CPU";
//          ServiceMonitor monitor = new ServiceMonitor(serviceName, 2000);
//
//          2. Подпишитесь на событие
//          monitor.ServiceStatusChanged += Monitor_ServiceStatusChanged
//
//          3. Запустите мониторинг службы (Вызвать 1 раз)
//          Task monitoringTask = monitor.StartMonitoringAsync();
//
//          4. По необходимости отключите мониторинг службы
//          monitor.StopMonitoring();
//
//          5. После отключения, подождите завершение мониторинга
//          await monitoringTask;
//      }
//
//-------------------------------------------------------------------------------

namespace serverCPUService
{
    internal class ServiceMonitor
    {
        private ServiceController serviceController;
        private string serviceName;
        private int pollIntervalMilliseconds; // Add poll interval

        // Add a CancellationTokenSource to stop the polling
        private CancellationTokenSource cancellationTokenSource;

        public event EventHandler<ServiceStatusChangedEventArgs> ServiceStatusChanged;

        public ServiceMonitor(string serviceName, int pollIntervalMilliseconds = 1000)
        {
            this.serviceName = serviceName;
            this.pollIntervalMilliseconds = pollIntervalMilliseconds;
            serviceController = new ServiceController(serviceName);
            cancellationTokenSource = new CancellationTokenSource(); // Initialize the CancellationTokenSource
        }

        public async Task StartMonitoringAsync()
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            ServiceControllerStatus previousStatus = serviceController.Status;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    serviceController.Refresh();
                    ServiceControllerStatus currentStatus = serviceController.Status;

                    if (currentStatus != previousStatus)
                    {
                        LogConsole.WriteLine($"Статус службы '{serviceName}' изменился с '{previousStatus}' на '{currentStatus}'");

                        // Raise the event
                        OnServiceStatusChanged(new ServiceStatusChangedEventArgs(previousStatus, currentStatus));

                        previousStatus = currentStatus;
                    }

                    await Task.Delay(pollIntervalMilliseconds, cancellationToken);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении статуса службы '{serviceName}': {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла непредвиденная ошибка при мониторинге службы '{serviceName}': {ex.Message}");
                    break;
                }
            }
            Console.WriteLine("Stopped monitoring the service.");
        }

        public void StopMonitoring()
        {
            cancellationTokenSource.Cancel();
            LogConsole.WriteLine("StopMonitoring called.");
        }

        protected virtual void OnServiceStatusChanged(ServiceStatusChangedEventArgs e)
        {
            ServiceStatusChanged?.Invoke(this, e);
        }

        public class ServiceStatusChangedEventArgs : EventArgs
        {
            public ServiceControllerStatus OldStatus { get; }
            public ServiceControllerStatus NewStatus { get; }

            public ServiceStatusChangedEventArgs(ServiceControllerStatus oldStatus, ServiceControllerStatus newStatus)
            {
                OldStatus = oldStatus;
                NewStatus = newStatus;
            }
        }
    }
}

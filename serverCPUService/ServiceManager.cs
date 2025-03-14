using System;
using System.ServiceProcess;
using System.Threading.Tasks;

//-------------------------------------------------
// Получение информации о конкретной службе
//-------------------------------------------------

//TODO: сделать методы ассинхронными
namespace serverCPUService
{
    internal class ServiceManager
    {
        public static ServiceController Service;

        public async static Task<bool> StopService(string serviceName, int timeoutMilliseconds = 50000)
        {
            return await Task.Run(() =>
            {
                ServiceController service = new ServiceController(serviceName);

                if (service.Status != ServiceControllerStatus.Stopped && service.Status != ServiceControllerStatus.StopPending)
                {
                    LogConsole.WriteLine($"Остановка службы '{serviceName}'...");

                    service.Stop();

                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(timeoutMilliseconds));
                    LogConsole.WriteLine($"Служба '{serviceName}' успешно остановлена.");
                    return true;
                }
                else
                {
                    LogConsole.WriteLine($"Служба '{serviceName}' уже остановлена или находится в процессе остановки.");
                    return true;
                }
            });
        }

        public async static Task<bool> StartService(string serviceName, int timeoutMilliseconds = 50000)
        {
            return await Task.Run(() =>
            {
                ServiceController service = new ServiceController(serviceName);

                if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)
                {
                    Console.WriteLine($"Запуск службы '{serviceName}'...");

                    service.Start();

                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(timeoutMilliseconds));
                    Console.WriteLine($"Служба '{serviceName}' успешно запущена.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Служба '{serviceName}' уже запущена или находится в процессе запуска.");
                    return true;
                }
            });
        }

        public async static  Task<bool> RestartService(string serviceName, int timeoutMilliseconds = 50000)
        {
            if (await StopService(serviceName, timeoutMilliseconds))
            {
                return await StartService(serviceName, timeoutMilliseconds);
            }
            else
            {
                Console.WriteLine($"Ошибка перезапуска службы '{serviceName}' потому что остановка не удалась.");
                return false;
            }
        }

        public async static Task<ServiceControllerStatus?> GetServiceStatus(string serviceName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ServiceController service = new ServiceController(serviceName);
                    return (ServiceControllerStatus?)service.Status;
                }
                catch (InvalidOperationException ex)
                {
                    LogConsole.WriteLine($"Служба '{serviceName}' не существует/установлена " +
                        $"или доступ к ней запрещён: {ex.Message}");
                    return null;
                }
            });
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace serverCPUService
{
    internal class AppLauncher
    {
        private static string _programPath = "";
        private static PerformanceCounter _cpuCounter;
        private static ProgramLauncher _programLauncher;
        private static bool _isRunning = false;

        private int _triggerCpuUsage;

        public AppLauncher(int durationSeconds, int triggerCpuUsage)
        {
            _programLauncher = new ProgramLauncher(_programPath, durationSeconds);
            _triggerCpuUsage = triggerCpuUsage;
        }

        public async Task StartMonitoringAsync()
        {
            _isRunning = true;

            while (_isRunning)
            {
                float cpuUsage = CpuCounter.GetAvarage();

                LogConsole.WriteLine($"CPU Usage: {cpuUsage:F2}%");

                if (cpuUsage >= _triggerCpuUsage && !_programLauncher.IsProgramRunning)
                {
                    await _programLauncher.LaunchProgramAsync();
                }

                Thread.Sleep(1000);
            }
        }

        public string SetProgramPath(string programPath)
        {
            if (string.IsNullOrEmpty(programPath))
            {
                programPath = null;
                return "Путь к программе не может быть нулевым или пустым.";
            }

            if (!File.Exists(programPath))
            {
                programPath = null;
                return $"Файл не найден: {programPath}";
            }

            _programPath = programPath;
            return "Успешно";
        }

        public void StopMonitoring()
        {
            _isRunning = false;
        }

        // Внутренний класс для запуска внешней программы и управления ею
        private class ProgramLauncher
        {
            private string _programPath;
            private int _durationSeconds;
            public bool IsProgramRunning { get; private set; }
            private DateTime _programStartTime;
            private Process _process;

            public ProgramLauncher(string programPath, int durationSeconds)
            {
                _programPath = programPath;
                _durationSeconds = durationSeconds;
                IsProgramRunning = false;
            }

            public async Task LaunchProgramAsync()
            {
                IsProgramRunning = true;  // предотвращение многократного запуска
                await Task.Run(() =>
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(_programPath);
                        _process = Process.Start(startInfo);

                        if (_process != null)
                        {
                            _programStartTime = DateTime.Now;
                            LogConsole.WriteLine($"Программа {_programPath} запустилась.");

                            while (DateTime.Now - _programStartTime < TimeSpan.FromSeconds(_durationSeconds))
                            {
                                Thread.Sleep(100);
                                if (_process.HasExited)
                                {
                                    break;
                                }
                            }

                            if (!_process.HasExited)
                            {
                                LogConsole.WriteLine("Время истекло.  Завершение работы программы...");
                                _process.Kill();
                            }

                            _process.WaitForExit();
                            LogConsole.WriteLine($"Программа {_programPath} завершена.");
                        }
                        else
                        {
                            LogConsole.WriteLine($"Не удалось запустить программу {_programPath}.");
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        if (_process != null)
                        {
                            _process.Dispose();
                            _process = null;
                        }

                        IsProgramRunning = false; 
                    }
                });
            }
        }
    }
}

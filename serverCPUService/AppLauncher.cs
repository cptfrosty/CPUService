using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

                Console.WriteLine($"CPU Usage: {cpuUsage:F2}%");

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

        // Inner class for launching and managing the external program
        private class ProgramLauncher
        {
            private string _programPath;
            private int _durationSeconds;
            public bool IsProgramRunning { get; private set; } // Public get, private set
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
                IsProgramRunning = true;  // set to true immediately to prevent multiple launches
                await Task.Run(() =>
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(_programPath);
                        _process = Process.Start(startInfo);

                        if (_process != null)
                        {
                            _programStartTime = DateTime.Now;
                            Console.WriteLine($"Program {_programPath} started.");

                            while (DateTime.Now - _programStartTime < TimeSpan.FromSeconds(_durationSeconds))
                            {
                                Thread.Sleep(100);
                                if (_process.HasExited)
                                {
                                    Console.WriteLine($"Program {_programPath} exited early.");
                                    break;
                                }
                            }

                            if (!_process.HasExited)
                            {
                                Console.WriteLine("Time elapsed.  Killing the program...");
                                _process.Kill();
                            }

                            _process.WaitForExit();  // Ensure process has fully exited
                            Console.WriteLine($"Program {_programPath} completed.");
                        }
                        else
                        {
                            Console.WriteLine($"Could not start program {_programPath}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error launching or running program: {ex.Message}");
                    }
                    finally
                    {
                        if (_process != null)
                        {
                            _process.Dispose(); // Release resources
                            _process = null;    // Important: Set to null
                        }

                        IsProgramRunning = false;  // Allow launching again
                    }
                });
            }
        }
    }
}

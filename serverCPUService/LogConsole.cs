using System.Runtime.InteropServices;
using System.Threading.Tasks;

//-------------------------------------------------------
//  Чтобы работал вывод в консоль необходимо
// запустить VS от админа или exe файл из папки.
//  Для вызова необходимо вызвать Init из другого класса
// и после можно использовать остальные методы
//-------------------------------------------------------

namespace serverCPUService
{
    internal class LogConsole
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole(); //Закрыть консоль

        public static void Init()
        {
            Task.Factory.StartNew(Console);
        }

        private static void Console()
        {
            // Запускаем консоль.
            if (AllocConsole())
            {
                System.Console.WriteLine("Для выхода наберите exit.");
            }
        }

        public static void WriteLine(string message)
        {
            if (AllocConsole())
            {
                WriteLineLocal(message);
            }
        }

        public static void WriteLine(string format, params object[] args)
        {
            if (AllocConsole())
            {
                WriteLineLocal(format, args);
            }
        }

        public static void Write(string message)
        {
            if (AllocConsole())
            {
                WriteLineLocal(message);
            }
        }

        public static void Write(string format, params object[] args)
        {
            if (AllocConsole())
            {
                WriteLineLocal(format, args);
            }
        }

        private static void WriteLineLocal(string message) => System.Console.WriteLine(message);
        private static void WriteLineLocal(string format, params object[] args) => System.Console.WriteLine(format, args);
        private static void WriteLocal (string message) => System.Console.Write(message);
        private static void WriteLocal(string format, params object[] args) => System.Console.Write(format, args);
    }
}

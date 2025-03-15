using System.Collections.Generic;
using System.Linq;

//--------------------------------------------------------------------
//  Функциональность "Сбор нагрузки ЦП":
// Происходит сбор значений где пул объектов опрелеляется через
// максимальный размер коллекции
//--------------------------------------------------------------------

namespace serverCPUService
{
    internal class CpuCounter
    {
        public static List<float> _averageCpuUsage { get; private set; }
        private static int _maxSizeCollection = 10;

        /// <summary>
        /// Получить среднее значение загруженности процессора
        /// </summary>
        /// <returns></returns>
        public static float GetAvarage() => _averageCpuUsage.Average();

        /// <summary>
        /// Добавить значение в пул
        /// </summary>
        /// <param name="value"></param>
        public static void Add(string value)
        {
            if(_averageCpuUsage == null) _averageCpuUsage = new List<float>();

            float cpuUsage;
            bool isParseCpuUsage = float.TryParse(value, out cpuUsage);

            //Для избавления сильной погрешности
            if (cpuUsage > 0.0f)
            {
                _averageCpuUsage.Add(cpuUsage);
            }

            if (_averageCpuUsage.Count > _maxSizeCollection)
            {
                _averageCpuUsage.RemoveAt(0);
            }
        }
    }
}

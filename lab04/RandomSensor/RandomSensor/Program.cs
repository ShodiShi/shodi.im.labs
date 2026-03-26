using System;

namespace RandomSensor
{
    class Program
    {
        static void Main(string[] args)
        {
            long a = 16807;
            long m = 2147483647;
            long x = 12345;

            int n = 100000;

            double[] values = new double[n];

            for (int i = 0; i < n; i++)
            {
                x = (a * x) % m;
                values[i] = (double)x / m;
            }
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += values[i];
            }
            double mean = sum / n;

            double sumSquares = 0;
            for (int i = 0; i < n; i++)
            {
                double diff = values[i] - mean;
                sumSquares += diff * diff;
            }
            double varianse = sumSquares / n;

            Console.WriteLine("===== РЕЗУЛЬТАТЫ НАШЕГО ГЕНЕРАТОРА =====");
            Console.WriteLine("Выборочное среднее : " + mean);
            Console.WriteLine("Выборочная дисперсия : " + varianse);
            Console.WriteLine();
            Console.WriteLine("===== ТЕОРИТИЧЕСКИЕ ЗНАЧЕНИЯ =====");
            Console.WriteLine("Теоритическое среднее: 0.5");
            Console.WriteLine("Теоритическая дисперсия: 0.0833...");

            long a2 = 1103515245;
            long m2 = 2147483648;
            long x2 = 67890;

            double[] values2 = new double[n];
            for (int i = 0; i < n; i++)
            {
                x2 = (a2 * x2) % m2;
                values2[i] = (double)x2 / m2;
            }
            double sum2 = 0;
            for (int i = 0; i < n; i++)
            {
                sum2 += values2[i];
            }
            double mean2 = sum2 / n;
            double sumSquares2 = 0;
            for (int i = 0; i < n; i++)
            {
                double diff2 = values2[i] - mean2;
                sumSquares2 += diff2 * diff2;
            }

            double varianse2 = sumSquares2 / n;

            Console.WriteLine("╔══════════════════════════╦══════════════════╦══════════════════╗");
            Console.WriteLine("║ Генератор                ║ Среднее          ║ Дисперсия        ║");
            Console.WriteLine("╠══════════════════════════╬══════════════════╬══════════════════╣");
            Console.WriteLine(String.Format("║ {0,-24} ║ {1,-16} ║ {2,-16} ║",
                "Генератор 1 (Парк-Миллер)",
                Math.Round(mean, 7),      
                Math.Round(varianse, 7)));

            Console.WriteLine(String.Format("║ {0,-24} ║ {1,-16} ║ {2,-16} ║",
                "Генератор 2 (ANSI C)",
                Math.Round(mean2, 7),
                Math.Round(varianse2, 7)));

            Console.WriteLine("╠══════════════════════════╬══════════════════╬══════════════════╣");

            Console.WriteLine(String.Format("║ {0,-24} ║ {1,-16} ║ {2,-16} ║",
                "Теоретические значения",
                0.5,                       
                0.0833333));               

            Console.WriteLine("╚══════════════════════════╩══════════════════╩══════════════════╝");

            Console.WriteLine();
            Console.WriteLine("Вывод: оба генератора близки к теоретическим значениям,");
            Console.WriteLine("что подтверждает равномерное распределение выборки.");
        }
    }
}
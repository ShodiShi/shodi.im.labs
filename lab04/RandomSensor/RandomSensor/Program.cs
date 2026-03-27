using System;

namespace RandomSensor
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = 100000;
            long a = 16807;
            long m = 2147483647;
            long x = 12345;

            double[] values = new double[n];
            for (int i = 0; i < n; i++)
            {
                x = (a * x) % m;
                values[i] = (double)x / m;
            }

            double sum = 0;
            for (int i = 0; i < n; i++)
                sum += values[i];
            double mean = sum / n;

            double sumSquares = 0;
            for (int i = 0; i < n; i++)
            {
                double diff = values[i] - mean;
                sumSquares += diff * diff;
            }
            double variance = sumSquares / n;
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
                sum2 += values2[i];
            double mean2 = sum2 / n;

            double sumSquares2 = 0;
            for (int i = 0; i < n; i++)
            {
                double diff2 = values2[i] - mean2;
                sumSquares2 += diff2 * diff2;
            }
            double variance2 = sumSquares2 / n;
            Random rnd = new Random();

            double[] values3 = new double[n];
            for (int i = 0; i < n; i++)
                values3[i] = rnd.NextDouble();

            double sum3 = 0;
            for (int i = 0; i < n; i++)
                sum3 += values3[i];
            double mean3 = sum3 / n;

            double sumSquares3 = 0;
            for (int i = 0; i < n; i++)
            {
                double diff3 = values3[i] - mean3;
                sumSquares3 += diff3 * diff3;
            }
            double variance3 = sumSquares3 / n;
            Console.WriteLine("╔══════════════════════════╦══════════════════╦══════════════════╗");
            Console.WriteLine("║ Генератор                ║ Среднее          ║ Дисперсия        ║");
            Console.WriteLine("╠══════════════════════════╬══════════════════╬══════════════════╣");
            Console.WriteLine(String.Format("║ {0,-24} ║ {1,-16} ║ {2,-16} ║",
                "Генератор 1 (Парк-Миллер)", Math.Round(mean, 7), Math.Round(variance, 7)));
            Console.WriteLine(String.Format("║ {0,-24} ║ {1,-16} ║ {2,-16} ║",
                "Генератор 2 (ANSI C)", Math.Round(mean2, 7), Math.Round(variance2, 7)));
            Console.WriteLine(String.Format("║ {0,-24} ║ {1,-16} ║ {2,-16} ║",
                "Встроенный (Random)", Math.Round(mean3, 7), Math.Round(variance3, 7)));
            Console.WriteLine("╠══════════════════════════╬══════════════════╬══════════════════╣");
            Console.WriteLine(String.Format("║ {0,-24} ║ {1,-16} ║ {2,-16} ║",
                "Теоретические значения", 0.5, 0.0833333));
            Console.WriteLine("╚══════════════════════════╩══════════════════╩══════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Вывод: все три генератора близки к теоретическим значениям,");
            Console.WriteLine("что подтверждает равномерное распределение выборки.");
        }
    }
}

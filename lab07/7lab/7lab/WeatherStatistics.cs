using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace lab7
{
    public class WeatherStatistics
    {
        public static readonly string[] StateNames = { "Ясно", "Облачно", "Пасмурно" };
        public static readonly string[] StateIcons = { "☀", "⛅", "☁" };

        public double[] Empirical { get; } = new double[3];
        public double[] Theoretical { get; } = new double[3];

        public void CalculateEmpirical(double[] stateTime, double totalTime)
        {
            if (totalTime <= 0)
            {
                throw new Exception("Общее время моделирования должно быть больше нуля.");
            }

            for (int i = 0; i < 3; i++)
            {
                Empirical[i] = stateTime[i] / totalTime;
            }
        }

        public void CalculateTheoretical(double[,] Q)
        {
            double[,] matrix =
            {
                { Q[0, 0], Q[1, 0], Q[2, 0] },
                { Q[0, 1], Q[1, 1], Q[2, 1] },
                { 1,       1,       1       }
            };

            double[] constants = { 0, 0, 1 };
            double det = Determinant3x3(matrix);

            if (Math.Abs(det) < 1e-9)
            {
                throw new Exception("Не удалось вычислить теоретическое распределение: определитель равен нулю.");
            }

            for (int i = 0; i < 3; i++)
            {
                Theoretical[i] = Determinant3x3(ReplaceColumn(matrix, constants, i)) / det;
            }
        }

        public List<string> BuildResultLines(double totalTime, int transitionCount)
        {
            List<string> lines = new List<string>();

            lines.Add("═══ РЕЗУЛЬТАТЫ МОДЕЛИРОВАНИЯ ═══");
            lines.Add($"  Период: {totalTime:F0} дн.  |  Переходов: {transitionCount}");
            lines.Add("────────────────────────────────────");
            lines.Add("  Состояние  │ Эмп.   │ Теор.  │   Δ");
            lines.Add("────────────────────────────────────");

            for (int i = 0; i < 3; i++)
            {
                double diff = Math.Abs(Empirical[i] - Theoretical[i]);
                lines.Add($"  {StateNames[i],-10} │ {Empirical[i]:F4} │ {Theoretical[i]:F4} │ {diff:F4}");
            }

            lines.Add("────────────────────────────────────");
            return lines;
        }

        public void SaveToCsv(
            string filePath,
            double totalTime,
            int initialState,
            double[,] Q,
            List<(double time, int state)> history,
            double[] stateTime)
        {
            using StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);

            sw.WriteLine("# Марковская модель погоды — результаты моделирования");
            sw.WriteLine($"# Дата и время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sw.WriteLine($"# Период моделирования: {totalTime} дней");
            sw.WriteLine($"# Начальное состояние: {StateNames[initialState]}");
            sw.WriteLine("#");
            sw.WriteLine("# Матрица интенсивностей Q:");
            sw.WriteLine($"# [ {Q[0, 0]:F3}  {Q[0, 1]:F3}  {Q[0, 2]:F3} ]");
            sw.WriteLine($"# [ {Q[1, 0]:F3}  {Q[1, 1]:F3}  {Q[1, 2]:F3} ]");
            sw.WriteLine($"# [ {Q[2, 0]:F3}  {Q[2, 1]:F3}  {Q[2, 2]:F3} ]");
            sw.WriteLine("#");
            sw.WriteLine("# === ИСТОРИЯ ПЕРЕХОДОВ ===");
            sw.WriteLine("Время;Номер состояния;Название состояния");

            foreach (var entry in history)
            {
                sw.WriteLine($"{entry.time:F4};{entry.state + 1};{StateNames[entry.state]}");
            }

            sw.WriteLine();
            sw.WriteLine("# === СТАТИСТИКА ПО СОСТОЯНИЯМ ===");
            sw.WriteLine("Состояние;Время пребывания (дн);Эмпирическая частота;Теоретическая частота;Отклонение");

            for (int i = 0; i < 3; i++)
            {
                double diff = Math.Abs(Empirical[i] - Theoretical[i]);
                sw.WriteLine($"{StateNames[i]};{stateTime[i]:F4};{Empirical[i]:F6};{Theoretical[i]:F6};{diff:F6}");
            }
        }

        private double Determinant3x3(double[,] m)
        {
            return
                m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1])
              - m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0])
              + m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
        }

        private double[,] ReplaceColumn(double[,] matrix, double[] column, int columnIndex)
        {
            double[,] result = (double[,])matrix.Clone();

            for (int i = 0; i < 3; i++)
            {
                result[i, columnIndex] = column[i];
            }

            return result;
        }
    }
}

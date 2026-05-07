using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace lab7
{
    public partial class Form1 : Form
    {
        private readonly WeatherSimulator _simulator = new WeatherSimulator();
        private readonly WeatherStatistics _statistics = new WeatherStatistics();

        private static readonly string SavePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "weather_results.csv");

        private bool _resultsReady;

        public Form1()
        {
            InitializeComponent();
            ConfigureForm();
        }

        private void ConfigureForm()
        {
            numericUpDown1.Enabled = false;
            numericUpDown5.Enabled = false;
            numericUpDown9.Enabled = false;

            numericUpDown2.ValueChanged += (s, e) => UpdateDiagonal();
            numericUpDown3.ValueChanged += (s, e) => UpdateDiagonal();
            numericUpDown4.ValueChanged += (s, e) => UpdateDiagonal();
            numericUpDown6.ValueChanged += (s, e) => UpdateDiagonal();
            numericUpDown7.ValueChanged += (s, e) => UpdateDiagonal();
            numericUpDown8.ValueChanged += (s, e) => UpdateDiagonal();

            timer1.Interval = 200;
            timer1.Tick += Timer_Tick;

            ConfigureChart();

            log_Day.Text = "—";
            label_initState.Text = "Нач. состояние:\n—";

            UpdateDiagonal();
        }

        private void ConfigureChart()
        {
            chart1.Series.Clear();
            chart1.Series.Add(CreateSeries("Ясно", Color.FromArgb(255, 179, 0)));
            chart1.Series.Add(CreateSeries("Облачно", Color.FromArgb(21, 101, 192)));
            chart1.Series.Add(CreateSeries("Пасмурно", Color.FromArgb(69, 90, 100)));

            chart1.ChartAreas[0].AxisX.Title = "Время (дни)";
            chart1.ChartAreas[0].AxisY.Title = "Частота";
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 1;
        }

        private Series CreateSeries(string name, Color color)
        {
            Series series = new Series(name);
            series.ChartArea = "ChartArea1";
            series.Legend = "Legend1";
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 3;
            series.Color = color;
            return series;
        }

        private void UpdateDiagonal()
        {
            numericUpDown1.Value = -(numericUpDown2.Value + numericUpDown3.Value);
            numericUpDown5.Value = -(numericUpDown4.Value + numericUpDown6.Value);
            numericUpDown9.Value = -(numericUpDown7.Value + numericUpDown8.Value);
        }

        private void ReadRatesFromForm()
        {
            double q12 = (double)numericUpDown2.Value;
            double q13 = (double)numericUpDown3.Value;
            double q21 = (double)numericUpDown4.Value;
            double q23 = (double)numericUpDown6.Value;
            double q31 = (double)numericUpDown7.Value;
            double q32 = (double)numericUpDown8.Value;

            if (q12 + q13 <= 0)
            {
                throw new Exception("В первой строке должна быть хотя бы одна положительная интенсивность.");
            }

            if (q21 + q23 <= 0)
            {
                throw new Exception("Во второй строке должна быть хотя бы одна положительная интенсивность.");
            }

            if (q31 + q32 <= 0)
            {
                throw new Exception("В третьей строке должна быть хотя бы одна положительная интенсивность.");
            }

            _simulator.SetRates(q12, q13, q21, q23, q31, q32);
        }

        private void UpdateCurrentStateView()
        {
            int state = _simulator.CurrentState;

            log_Day.Text =
                $"{WeatherStatistics.StateIcons[state]} {WeatherStatistics.StateNames[state]}\n" +
                $"День: {_simulator.GetCurrentDay()}\n" +
                $"t = {_simulator.CurrentTime:F1} дн.";
        }

        private void UpdateChart()
        {
            if (_simulator.CurrentTime <= 0)
            {
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                double frequency = _simulator.StateTime[i] / _simulator.CurrentTime;
                chart1.Series[i].Points.AddXY(_simulator.CurrentTime, frequency);
            }
        }

        private void ShowResults()
        {
            _statistics.CalculateEmpirical(_simulator.StateTime, _simulator.CurrentTime);
            _statistics.CalculateTheoretical(_simulator.Q);

            listBox1.Items.Clear();

            foreach (string line in _statistics.BuildResultLines(_simulator.TotalTime, _simulator.History.Count))
            {
                listBox1.Items.Add(line);
            }

            listBox1.Items.Add("  Файл сохранения:");
            listBox1.Items.Add($"  {Path.GetFileName(SavePath)}");
        }

        private void SaveResults()
        {
            _statistics.SaveToCsv(
                SavePath,
                _simulator.TotalTime,
                _simulator.InitialState,
                _simulator.Q,
                _simulator.History,
                _simulator.StateTime);
        }

        private void Start_Click(object? sender, EventArgs e)
        {
            try
            {
                timer1.Stop();
                ReadRatesFromForm();
                _simulator.Start((double)T.Value);
                _resultsReady = false;

                foreach (Series series in chart1.Series)
                {
                    series.Points.Clear();
                }

                listBox1.Items.Clear();

                label_initState.Text =
                    $"Нач. состояние:\n{WeatherStatistics.StateIcons[_simulator.InitialState]} " +
                    $"{WeatherStatistics.StateNames[_simulator.InitialState]}\n" +
                    $"λ = {-_simulator.Q[_simulator.InitialState, _simulator.InitialState]:F2} дн⁻¹";

                UpdateCurrentStateView();
                timer1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bool shouldContinue = _simulator.Step();

            UpdateCurrentStateView();
            UpdateChart();

            if (!shouldContinue)
            {
                timer1.Stop();

                try
                {
                    ShowResults();
                    SaveResults();
                    _resultsReady = true;

                    MessageBox.Show(
                        $"Моделирование завершено.\n\nФайл сохранен:\n{SavePath}",
                        "Готово",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object? sender, EventArgs e)
        {
            timer1.Stop();
            _resultsReady = false;

            foreach (Series series in chart1.Series)
            {
                series.Points.Clear();
            }

            _simulator.History.Clear();
            Array.Clear(_simulator.StateTime, 0, _simulator.StateTime.Length);

            listBox1.Items.Clear();
            log_Day.Text = "—";
            label_initState.Text = "Нач. состояние:\n—";
        }

        private void save_button_Click(object? sender, EventArgs e)
        {
            if (!_resultsReady)
            {
                MessageBox.Show(
                    "Сначала завершите моделирование.",
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                SaveResults();

                MessageBox.Show(
                    $"Данные сохранены:\n{SavePath}",
                    "Сохранено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}

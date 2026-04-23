using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab6
{
    public partial class Form1 : Form
    {
        private readonly double[] xValues = { 1, 2, 3, 4, 5 };
        private readonly double[] pValues = { 0.10, 0.20, 0.40, 0.20, 0.10 };
        private const double CHI2_CRITICAL = 9.488;

        private Label? labelAvg;
        private Label? labelVar;
        private Label? labelChi;
        private Label? labelConclusion;
        private Label? labelStats;
        private Label? labelNormalResult;
        private Panel? panelHistogram;

        private int[]? histogramData;
        private double histogramMin;
        private double histogramMax;
        private double mu;
        private double sigma;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Лабораторная работа №6 — Моделирование ДСВ";
            Size = new Size(620, 970);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            Label title = new Label();
            title.Text = "Моделирование дискретной случайной величины";
            title.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            title.Location = new Point(20, 20);
            title.AutoSize = true;
            Controls.Add(title);

            Label labelTable = new Label();
            labelTable.Text = "Ряд распределения:";
            labelTable.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            labelTable.Location = new Point(20, 60);
            labelTable.AutoSize = true;
            Controls.Add(labelTable);

            string rowX = "X:       ";
            string rowP = "P:       ";
            for (int i = 0; i < xValues.Length; i++)
            {
                rowX += xValues[i].ToString().PadRight(8);
                rowP += pValues[i].ToString("F2").PadRight(8);
            }

            Label labelRowX = new Label();
            labelRowX.Text = rowX;
            labelRowX.Font = new Font("Courier New", 10f);
            labelRowX.Location = new Point(20, 85);
            labelRowX.AutoSize = true;
            Controls.Add(labelRowX);

            Label labelRowP = new Label();
            labelRowP.Text = rowP;
            labelRowP.Font = new Font("Courier New", 10f);
            labelRowP.Location = new Point(20, 105);
            labelRowP.AutoSize = true;
            Controls.Add(labelRowP);

            Label labelN = new Label();
            labelN.Text = "Количество экспериментов N:";
            labelN.Font = new Font("Segoe UI", 10f);
            labelN.Location = new Point(20, 140);
            labelN.AutoSize = true;
            Controls.Add(labelN);

            ComboBox comboN = new ComboBox();
            comboN.Location = new Point(20, 165);
            comboN.Size = new Size(150, 30);
            comboN.Font = new Font("Segoe UI", 10f);
            comboN.DropDownStyle = ComboBoxStyle.DropDownList;
            comboN.Items.AddRange(new object[] { "10", "100", "1000", "10000" });
            comboN.SelectedIndex = 0;
            Controls.Add(comboN);

            Button btnStart = new Button();
            btnStart.Text = "Старт";
            btnStart.Font = new Font("Segoe UI", 11f);
            btnStart.Location = new Point(180, 162);
            btnStart.Size = new Size(120, 34);
            btnStart.BackColor = Color.SteelBlue;
            btnStart.ForeColor = Color.White;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Click += (s, e) => RunSimulation(comboN);
            Controls.Add(btnStart);

            Panel sep = new Panel();
            sep.Location = new Point(20, 210);
            sep.Size = new Size(560, 2);
            sep.BackColor = Color.LightGray;
            Controls.Add(sep);

            labelAvg = new Label();
            labelAvg.Text = "Среднее: —";
            labelAvg.Font = new Font("Segoe UI", 10f);
            labelAvg.Location = new Point(20, 225);
            labelAvg.AutoSize = true;
            Controls.Add(labelAvg);

            labelVar = new Label();
            labelVar.Text = "Дисперсия: —";
            labelVar.Font = new Font("Segoe UI", 10f);
            labelVar.Location = new Point(20, 250);
            labelVar.AutoSize = true;
            Controls.Add(labelVar);

            labelChi = new Label();
            labelChi.Text = "χ²: —";
            labelChi.Font = new Font("Segoe UI", 10f);
            labelChi.Location = new Point(20, 275);
            labelChi.AutoSize = true;
            Controls.Add(labelChi);

            labelConclusion = new Label();
            labelConclusion.Text = "Вывод: —";
            labelConclusion.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            labelConclusion.Location = new Point(20, 300);
            labelConclusion.AutoSize = true;
            Controls.Add(labelConclusion);

            labelStats = new Label();
            labelStats.Text = "Эмпирические вероятности появятся здесь...";
            labelStats.Font = new Font("Courier New", 9f);
            labelStats.ForeColor = Color.DimGray;
            labelStats.Location = new Point(20, 335);
            labelStats.AutoSize = true;
            Controls.Add(labelStats);

            Panel sep2 = new Panel();
            sep2.Location = new Point(20, 480);
            sep2.Size = new Size(560, 2);
            sep2.BackColor = Color.LightGray;
            Controls.Add(sep2);

            Label titleNormal = new Label();
            titleNormal.Text = "Моделирование нормального распределения";
            titleNormal.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            titleNormal.Location = new Point(20, 495);
            titleNormal.AutoSize = true;
            Controls.Add(titleNormal);

            Label labelParams = new Label();
            labelParams.Text = "Параметры: μ (среднее) = 5,  σ² (дисперсия) = 1";
            labelParams.Font = new Font("Segoe UI", 9f);
            labelParams.ForeColor = Color.DimGray;
            labelParams.Location = new Point(20, 525);
            labelParams.AutoSize = true;
            Controls.Add(labelParams);

            Label labelN2 = new Label();
            labelN2.Text = "Количество экспериментов N:";
            labelN2.Font = new Font("Segoe UI", 10f);
            labelN2.Location = new Point(20, 555);
            labelN2.AutoSize = true;
            Controls.Add(labelN2);

            ComboBox comboN2 = new ComboBox();
            comboN2.Location = new Point(20, 580);
            comboN2.Size = new Size(150, 30);
            comboN2.Font = new Font("Segoe UI", 10f);
            comboN2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboN2.Items.AddRange(new object[] { "100", "1000", "10000" });
            comboN2.SelectedIndex = 1;
            Controls.Add(comboN2);

            Button btnNormal = new Button();
            btnNormal.Text = "Построить";
            btnNormal.Font = new Font("Segoe UI", 11f);
            btnNormal.Location = new Point(180, 577);
            btnNormal.Size = new Size(120, 34);
            btnNormal.BackColor = Color.SeaGreen;
            btnNormal.ForeColor = Color.White;
            btnNormal.FlatStyle = FlatStyle.Flat;
            btnNormal.Click += (s, e) => GenerateNormal(comboN2);
            Controls.Add(btnNormal);

            labelNormalResult = new Label();
            labelNormalResult.Text = "Результаты появятся здесь...";
            labelNormalResult.Font = new Font("Segoe UI", 9f);
            labelNormalResult.ForeColor = Color.DimGray;
            labelNormalResult.Location = new Point(20, 625);
            labelNormalResult.AutoSize = true;
            Controls.Add(labelNormalResult);

            panelHistogram = new Panel();
            panelHistogram.Location = new Point(20, 655);
            panelHistogram.Size = new Size(560, 250);
            panelHistogram.BackColor = Color.White;
            panelHistogram.BorderStyle = BorderStyle.FixedSingle;
            panelHistogram.Paint += PanelHistogram_Paint;
            Controls.Add(panelHistogram);
        }

        private void RunSimulation(ComboBox comboN)
        {
            string? selectedItem = comboN.SelectedItem?.ToString();
            if (!int.TryParse(selectedItem, out int n))
            {
                MessageBox.Show("Ошибка: некорректное значение N");
                return;
            }

            DiscreteSimulationResult result = SimulationService.RunDiscrete(n, xValues, pValues);

            labelAvg!.Text =
                $"Среднее:    теория = {result.TheoreticalAverage:F3},  эмпирика = {result.EmpiricalAverage:F3},  погрешность = {result.AverageError:F1}%";

            labelVar!.Text =
                $"Дисперсия:  теория = {result.TheoreticalVariance:F3},  эмпирика = {result.EmpiricalVariance:F3},  погрешность = {result.VarianceError:F1}%";

            labelChi!.Text = $"χ² = {result.ChiSquare:F3}  (критическое = {CHI2_CRITICAL})";

            if (result.ChiSquare < CHI2_CRITICAL)
            {
                labelConclusion!.Text =
                    $"Вывод: χ² = {result.ChiSquare:F3} < {CHI2_CRITICAL} → гипотеза ПРИНЯТА";
                labelConclusion.ForeColor = Color.Green;
            }
            else
            {
                labelConclusion!.Text =
                    $"Вывод: χ² = {result.ChiSquare:F3} > {CHI2_CRITICAL} → гипотеза ОТВЕРГНУТА";
                labelConclusion.ForeColor = Color.Red;
            }

            string stats = "X\t ni\t pi(теор)\t pi(эмп)\n";
            stats += "--\t---\t--------\t-------\n";

            for (int i = 0; i < xValues.Length; i++)
            {
                double empP = (double)result.Counts[i] / n;
                stats += $"{xValues[i]}\t{result.Counts[i]}\t{pValues[i]:F2}\t\t{empP:F4}\n";
            }

            labelStats!.Text = stats;
        }

        private void GenerateNormal(ComboBox comboN)
        {
            string? selectedItem = comboN.SelectedItem?.ToString();
            if (!int.TryParse(selectedItem, out int n))
            {
                MessageBox.Show("Ошибка: некорректное значение N");
                return;
            }

            NormalSimulationResult result = SimulationService.RunNormal(n, 5.0, 1.0);

            mu = result.Mu;
            sigma = result.Sigma;
            histogramData = result.HistogramData;
            histogramMin = result.HistogramMin;
            histogramMax = result.HistogramMax;

            labelNormalResult!.Text =
                $"N = {n}    |    " +
                $"μ: теор = 5.00, эмп = {result.Mu:F4}, погр = {result.MuError:F2}%    |    " +
                $"σ: теор = 1.00, эмп = {result.Sigma:F4}, погр = {result.SigmaError:F2}%";

            panelHistogram?.Invalidate();
        }

        private void PanelHistogram_Paint(object? sender, PaintEventArgs e)

        {
            if (histogramData == null || panelHistogram == null)
                return;

            HistogramRenderer.Draw(
                e.Graphics,
                panelHistogram,
                histogramData,
                histogramMin,
                histogramMax,
                mu,
                sigma);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

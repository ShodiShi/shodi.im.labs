using System;
using System.Drawing;
using System.Windows.Forms;

namespace LabPoisson
{
    public class MainForm : Form
    {
        private readonly PoissonSimulator simulator = new PoissonSimulator();

        private Label labelT = new Label();
        private Label labelLambda = new Label();
        private Label labelExperiments = new Label();

        private NumericUpDown numericT = new NumericUpDown();
        private NumericUpDown numericLambda = new NumericUpDown();
        private NumericUpDown numericExperiments = new NumericUpDown();

        private Button buttonRun = new Button();
        private Button buttonExit = new Button();

        private ListBox listBoxResults = new ListBox();
        private PictureBox pictureBoxGraph = new PictureBox();

        private SimulationResult? lastResult;

        public MainForm()
        {
            InitializeForm();
            CreateControls();
        }

        private void InitializeForm()
        {
            Text = "Пуассоновский поток запросов";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 700);
            MinimumSize = new Size(1100, 700);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void CreateControls()
        {
            int labelLeft = 20;
            int valueLeft = 230;
            int top = 20;
            int rowHeight = 40;
            int labelWidth = 190;
            int valueWidth = 120;

            labelT.Text = "Интервал T:";
            labelT.Location = new Point(labelLeft, top);
            labelT.Size = new Size(labelWidth, 25);

            numericT.Location = new Point(valueLeft, top - 2);
            numericT.Size = new Size(valueWidth, 25);
            numericT.DecimalPlaces = 1;
            numericT.Minimum = 0.1M;
            numericT.Maximum = 1000M;
            numericT.Value = 10.0M;
            numericT.Increment = 0.1M;

            labelLambda.Text = "Интенсивность lambda:";
            labelLambda.Location = new Point(labelLeft, top + rowHeight);
            labelLambda.Size = new Size(labelWidth, 25);

            numericLambda.Location = new Point(valueLeft, top + rowHeight - 2);
            numericLambda.Size = new Size(valueWidth, 25);
            numericLambda.DecimalPlaces = 1;
            numericLambda.Minimum = 0.1M;
            numericLambda.Maximum = 100M;
            numericLambda.Value = 2.0M;
            numericLambda.Increment = 0.1M;

            labelExperiments.Text = "Число экспериментов:";
            labelExperiments.Location = new Point(labelLeft, top + rowHeight * 2);
            labelExperiments.Size = new Size(labelWidth, 25);

            numericExperiments.Location = new Point(valueLeft, top + rowHeight * 2 - 2);
            numericExperiments.Size = new Size(valueWidth, 25);
            numericExperiments.Minimum = 1;
            numericExperiments.Maximum = 100000;
            numericExperiments.Value = 1000;
            numericExperiments.Increment = 100;

            buttonRun.Text = "Запустить";
            buttonRun.Location = new Point(20, 145);
            buttonRun.Size = new Size(130, 35);
            buttonRun.Click += ButtonRun_Click;

            buttonExit.Text = "Выход";
            buttonExit.Location = new Point(170, 145);
            buttonExit.Size = new Size(130, 35);
            buttonExit.Click += ButtonExit_Click;

            listBoxResults.Location = new Point(20, 200);
            listBoxResults.Size = new Size(320, 450);
            listBoxResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            pictureBoxGraph.Location = new Point(360, 20);
            pictureBoxGraph.Size = new Size(720, 630);
            pictureBoxGraph.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBoxGraph.BackColor = Color.White;
            pictureBoxGraph.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxGraph.Paint += PictureBoxGraph_Paint;

            Controls.Add(labelT);
            Controls.Add(numericT);
            Controls.Add(labelLambda);
            Controls.Add(numericLambda);
            Controls.Add(labelExperiments);
            Controls.Add(numericExperiments);
            Controls.Add(buttonRun);
            Controls.Add(buttonExit);
            Controls.Add(listBoxResults);
            Controls.Add(pictureBoxGraph);
        }

        private void ButtonRun_Click(object? sender, EventArgs e)
        {
            listBoxResults.Items.Clear();

            double T = (double)numericT.Value;
            double lambda = (double)numericLambda.Value;
            int experimentsCount = (int)numericExperiments.Value;

            lastResult = simulator.RunSimulation(lambda, T, experimentsCount);

            listBoxResults.Items.Add("Параметры моделирования:");
            listBoxResults.Items.Add($"T = {T:F1}");
            listBoxResults.Items.Add($"lambda = {lambda:F1}");
            listBoxResults.Items.Add($"Число экспериментов = {experimentsCount}");
            listBoxResults.Items.Add("");

            listBoxResults.Items.Add("Эмпирическое распределение:");
            for (int k = 0; k < lastResult.EmpiricalDistribution.Length; k++)
            {
                if (lastResult.EmpiricalDistribution[k] > 0)
                {
                    listBoxResults.Items.Add($"P(X = {k}) = {lastResult.EmpiricalDistribution[k]:F4}");
                }
            }

            listBoxResults.Items.Add("");
            listBoxResults.Items.Add("Эмпирические характеристики:");
            listBoxResults.Items.Add($"Среднее = {lastResult.EmpiricalMean:F4}");
            listBoxResults.Items.Add($"Дисперсия = {lastResult.EmpiricalVariance:F4}");

            listBoxResults.Items.Add("");
            listBoxResults.Items.Add("Теоретические характеристики:");
            listBoxResults.Items.Add($"Среднее = {lastResult.TheoreticalMean:F4}");
            listBoxResults.Items.Add($"Дисперсия = {lastResult.TheoreticalVariance:F4}");

            pictureBoxGraph.Invalidate();
        }

        private void ButtonExit_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void PictureBoxGraph_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            if (lastResult == null || lastResult.EmpiricalDistribution.Length == 0)
            {
                return;
            }

            Rectangle plotArea = new Rectangle(60, 30, pictureBoxGraph.Width - 90, pictureBoxGraph.Height - 80);

            using Pen axisPen = new Pen(Color.Black, 1);
            using Brush textBrush = new SolidBrush(Color.Black);
            using Brush empiricalBrush = new SolidBrush(Color.SteelBlue);
            using Pen theoreticalPen = new Pen(Color.Firebrick, 2);
            using Font font = new Font("Segoe UI", 9);
            using StringFormat centerFormat = new StringFormat();

            centerFormat.Alignment = StringAlignment.Center;

            g.DrawRectangle(axisPen, plotArea);

            double maxY = 0.0;

            for (int i = 0; i < lastResult.EmpiricalDistribution.Length; i++)
            {
                if (lastResult.EmpiricalDistribution[i] > maxY)
                {
                    maxY = lastResult.EmpiricalDistribution[i];
                }
            }

            for (int i = 0; i < lastResult.TheoreticalDistribution.Length; i++)
            {
                if (lastResult.TheoreticalDistribution[i] > maxY)
                {
                    maxY = lastResult.TheoreticalDistribution[i];
                }
            }

            if (maxY <= 0)
            {
                maxY = 1.0;
            }

            int count = lastResult.EmpiricalDistribution.Length;
            float stepX = (float)plotArea.Width / count;
            float barWidth = stepX * 0.6f;

            if (barWidth < 2f)
            {
                barWidth = 2f;
            }

            for (int i = 0; i < count; i++)
            {
                double p = lastResult.EmpiricalDistribution[i];
                float barHeight = (float)(p / maxY * plotArea.Height);
                float x = plotArea.Left + i * stepX + (stepX - barWidth) / 2f;
                float y = plotArea.Bottom - barHeight;

                g.FillRectangle(empiricalBrush, x, y, barWidth, barHeight);
            }

            if (count == 1)
            {
                float x = plotArea.Left + stepX / 2f;
                float y = plotArea.Bottom - (float)(lastResult.TheoreticalDistribution[0] / maxY * plotArea.Height);
                g.DrawEllipse(theoreticalPen, x - 2, y - 2, 4, 4);
            }
            else
            {
                PointF[] points = new PointF[count];

                for (int i = 0; i < count; i++)
                {
                    float x = plotArea.Left + i * stepX + stepX / 2f;
                    float y = plotArea.Bottom - (float)(lastResult.TheoreticalDistribution[i] / maxY * plotArea.Height);
                    points[i] = new PointF(x, y);
                }

                g.DrawLines(theoreticalPen, points);
            }

            int labelStep = Math.Max(1, count / 15);

            for (int i = 0; i < count; i += labelStep)
            {
                float x = plotArea.Left + i * stepX + stepX / 2f;
                g.DrawString(i.ToString(), font, textBrush, x, plotArea.Bottom + 5, centerFormat);
            }

            g.DrawString("Число запросов k", font, textBrush, plotArea.Left + plotArea.Width / 2f, pictureBoxGraph.Height - 25, centerFormat);
            g.DrawString("Вероятность", font, textBrush, 8, 10);
            g.DrawString(maxY.ToString("F2"), font, textBrush, 10, plotArea.Top - 5);

            g.FillRectangle(empiricalBrush, plotArea.Right - 220, plotArea.Top + 10, 20, 12);
            g.DrawString("Эмпирическое", font, textBrush, plotArea.Right - 190, plotArea.Top + 7);
            g.DrawLine(theoreticalPen, plotArea.Right - 220, plotArea.Top + 35, plotArea.Right - 200, plotArea.Top + 35);
            g.DrawString("Теоретическое", font, textBrush, plotArea.Right - 190, plotArea.Top + 28);
        }
    }
}
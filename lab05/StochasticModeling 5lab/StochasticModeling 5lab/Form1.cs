using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace lab5
{
    public partial class Form1 : Form
    {
   
        static long a = 16807;          
        static long m = 2147483647;      
        static long x = 12345;           

        static double NextDouble()
        {
            x = (a * x) % m;
            return (double)x / m;
        }

        const double P_YES = 0.5;
        int countYes = 0;
        int countNo = 0;

        string[] answers =
        {
            "Определённо да",
            "Без сомнений",
            "Можешь быть уверен",
            "Скорее всего да",
            "Знаки указывают на да",
            "Спроси позже",
            "Лучше не говорить сейчас",
            "Сложно сказать",
            "Не рассчитывай на это",
            "Мой ответ — нет",
            "Источники говорят нет",
            "Перспективы не очень",
            "Очень сомнительно"
        };

        double[] probabilities =
        {
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077,
            0.077
        };

        int[] countAnswers;
        Label labelAnswer1;
        Label labelStats1;
        Label labelAnswer2;
        Label labelStats2;

        public Form1()
        {
            InitializeComponent();
            countAnswers = new int[answers.Length];
            BuildUI();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void BuildUI()
        {
            this.Text = "Стохастическое моделирование";
            this.Size = new Size(700, 850);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.MaximizeBox = false;

            Panel panel1 = new Panel();
            panel1.BackColor = Color.FromArgb(33, 150, 243);
            panel1.Location = new Point(0, 0);
            panel1.Size = new Size(700, 380);
            this.Controls.Add(panel1);

            Label titlePart1 = new Label();
            titlePart1.Text = "Часть 1: Да или нет";
            titlePart1.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            titlePart1.ForeColor = Color.White;
            titlePart1.Location = new Point(30, 20);
            titlePart1.AutoSize = true;
            panel1.Controls.Add(titlePart1);

            Button btnAsk1 = new Button();
            btnAsk1.Text = "Спросить";
            btnAsk1.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnAsk1.Location = new Point(30, 65);
            btnAsk1.Size = new Size(160, 45);
            btnAsk1.BackColor = Color.FromArgb(255, 152, 0);
            btnAsk1.ForeColor = Color.White;
            btnAsk1.FlatStyle = FlatStyle.Flat;
            btnAsk1.FlatAppearance.BorderSize = 0;
            btnAsk1.Cursor = Cursors.Hand;
            btnAsk1.Click += btnAsk1_Click;
            panel1.Controls.Add(btnAsk1);

            Button btnReset1 = new Button();
            btnReset1.Text = "Сброс";
            btnReset1.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnReset1.Location = new Point(200, 65);
            btnReset1.Size = new Size(160, 45);
            btnReset1.BackColor = Color.FromArgb(244, 67, 54);
            btnReset1.ForeColor = Color.White;
            btnReset1.FlatStyle = FlatStyle.Flat;
            btnReset1.FlatAppearance.BorderSize = 0;
            btnReset1.Cursor = Cursors.Hand;
            btnReset1.Click += btnReset1_Click;
            panel1.Controls.Add(btnReset1);

            labelAnswer1 = new Label();
            labelAnswer1.Text = "?";
            labelAnswer1.Font = new Font("Segoe UI", 48f, FontStyle.Bold);
            labelAnswer1.ForeColor = Color.White;
            labelAnswer1.Location = new Point(30, 130);
            labelAnswer1.AutoSize = true;
            panel1.Controls.Add(labelAnswer1);

            labelStats1 = new Label();
            labelStats1.Text = "Да: 0    Нет: 0    Всего: 0";
            labelStats1.Font = new Font("Segoe UI", 11f);
            labelStats1.ForeColor = Color.FromArgb(200, 210, 230);
            labelStats1.Location = new Point(30, 210);
            labelStats1.AutoSize = true;
            panel1.Controls.Add(labelStats1);

            Panel panel2 = new Panel();
            panel2.BackColor = Color.FromArgb(156, 39, 176);
            panel2.Location = new Point(0, 380);
            panel2.Size = new Size(700, 470);
            this.Controls.Add(panel2);

            Label titlePart2 = new Label();
            titlePart2.Text = "Часть 2: Шар предсказаний";
            titlePart2.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            titlePart2.ForeColor = Color.White;
            titlePart2.Location = new Point(30, 20);
            titlePart2.AutoSize = true;
            panel2.Controls.Add(titlePart2);

            Button btnAsk2 = new Button();
            btnAsk2.Text = "Спросить шар";
            btnAsk2.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnAsk2.Location = new Point(30, 70);
            btnAsk2.Size = new Size(160, 45);
            btnAsk2.BackColor = Color.FromArgb(255, 152, 0);
            btnAsk2.ForeColor = Color.White;
            btnAsk2.FlatStyle = FlatStyle.Flat;
            btnAsk2.FlatAppearance.BorderSize = 0;
            btnAsk2.Cursor = Cursors.Hand;
            btnAsk2.Click += btnAsk2_Click;
            panel2.Controls.Add(btnAsk2);

            Button btnReset2 = new Button();
            btnReset2.Text = "Сброс";
            btnReset2.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnReset2.Location = new Point(200, 70);
            btnReset2.Size = new Size(160, 45);
            btnReset2.BackColor = Color.FromArgb(244, 67, 54);
            btnReset2.ForeColor = Color.White;
            btnReset2.FlatStyle = FlatStyle.Flat;
            btnReset2.FlatAppearance.BorderSize = 0;
            btnReset2.Cursor = Cursors.Hand;
            btnReset2.Click += btnReset2_Click;
            panel2.Controls.Add(btnReset2);

            labelAnswer2 = new Label();
            labelAnswer2.Text = "🔮 ?";
            labelAnswer2.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            labelAnswer2.ForeColor = Color.White;
            labelAnswer2.Location = new Point(30, 135);
            labelAnswer2.AutoSize = true;
            panel2.Controls.Add(labelAnswer2);

            labelStats2 = new Label();
            labelStats2.Text = "Всего вопросов: 0";
            labelStats2.Font = new Font("Segoe UI", 10f);
            labelStats2.ForeColor = Color.FromArgb(200, 210, 230);
            labelStats2.Location = new Point(30, 180);
            labelStats2.AutoSize = true;
            labelStats2.MaximumSize = new Size(640, 280);
            panel2.Controls.Add(labelStats2);
        }

        private void btnAsk1_Click(object sender, EventArgs e)
        {
            double alpha = NextDouble();
            if (alpha < P_YES)
            {
                countYes++;
                labelAnswer1.Text = "ДА";
                labelAnswer1.ForeColor = Color.FromArgb(76, 175, 80);
            }
            else
            {
                countNo++;
                labelAnswer1.Text = "НЕТ";
                labelAnswer1.ForeColor = Color.FromArgb(244, 67, 54);
            }
            int total = countYes + countNo;
            labelStats1.Text = $"ДА: {countYes}    НЕТ: {countNo}    ВСЕГО: {total}";
        }

        private void btnReset1_Click(object sender, EventArgs e)
        {
            countYes = 0;
            countNo = 0;
            labelAnswer1.Text = "?";
            labelAnswer1.ForeColor = Color.White;
            labelStats1.Text = "ДА: 0    НЕТ: 0    ВСЕГО: 0";
        }

        private void btnAsk2_Click(object sender, EventArgs e)
        {
            double alpha = NextDouble();
            double cumulativeProbability = 0.0;
            int selectedIndex = 0;

            for (int k = 0; k < probabilities.Length; k++)
            {
                cumulativeProbability += probabilities[k];
                if (alpha < cumulativeProbability)
                {
                    selectedIndex = k;
                    break;
                }
            }

            labelAnswer2.Text = $"🔮 {answers[selectedIndex]}";
            countAnswers[selectedIndex]++;

            int total = 0;
            foreach (int c in countAnswers) total += c;
            string stats = $"ВСЕГО ВОПРОСОВ: {total}\n\n";
            for (int i = 0; i < answers.Length; i++)
            {
                double freq = total > 0 ? (double)countAnswers[i] / total : 0;
                stats += $"{answers[i]}: {countAnswers[i]} ({freq:P1})\n";
            }
            labelStats2.Text = stats;
        }

        private void btnReset2_Click(object sender, EventArgs e)
        {
            Array.Clear(countAnswers, 0, countAnswers.Length);
            labelAnswer2.Text = "🔮 ?";
            labelStats2.Text = "ВСЕГО ВОПРОСОВ: 0";
        }
    }
}
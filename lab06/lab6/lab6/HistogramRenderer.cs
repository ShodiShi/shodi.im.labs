using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab6
{
    public static class HistogramRenderer
    {
        public static void Draw(
            Graphics g,
            Panel panel,
            int[] histogramData,
            double histogramMin,
            double histogramMax,
            double mu,
            double sigma)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int panelW = panel.Width;
            int panelH = panel.Height;
            int marginLeft = 45;
            int marginRight = 15;
            int marginTop = 15;
            int marginBottom = 35;
            int drawW = panelW - marginLeft - marginRight;
            int drawH = panelH - marginTop - marginBottom;

            int maxCount = 0;
            for (int i = 0; i < histogramData.Length; i++)
                if (histogramData[i] > maxCount)
                    maxCount = histogramData[i];

            if (maxCount == 0)
                return;

            float barW = (float)drawW / histogramData.Length;

            using Brush barBrush = new SolidBrush(Color.FromArgb(180, 70, 130, 200));
            using Pen barBorder = new Pen(Color.SteelBlue, 1f);

            for (int i = 0; i < histogramData.Length; i++)
            {
                float barH = (float)histogramData[i] / maxCount * drawH;
                float x = marginLeft + i * barW;
                float y = marginTop + drawH - barH;

                g.FillRectangle(barBrush, x, y, barW - 1, barH);
                g.DrawRectangle(barBorder, x, y, barW - 1, barH);
            }

            using Pen axisPen = new Pen(Color.Black, 1.5f);
            g.DrawLine(axisPen, marginLeft, marginTop, marginLeft, marginTop + drawH);
            g.DrawLine(axisPen, marginLeft, marginTop + drawH, marginLeft + drawW, marginTop + drawH);

            using Font axisFont = new Font("Segoe UI", 7.5f);
            Brush textBrush = Brushes.Black;

            using StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;

            for (int t = 0; t <= 4; t++)
            {
                double val = histogramMin + (histogramMax - histogramMin) * t / 4.0;
                float xPos = marginLeft + drawW * t / 4.0f;
                g.DrawString(val.ToString("F1"), axisFont, textBrush, xPos, marginTop + drawH + 5, sf);
            }

            sf.Alignment = StringAlignment.Far;
            g.DrawString("0", axisFont, textBrush, marginLeft - 4, marginTop + drawH - 7, sf);
            g.DrawString(maxCount.ToString(), axisFont, textBrush, marginLeft - 4, marginTop, sf);

            double fPeak = 1.0 / (sigma * Math.Sqrt(2 * Math.PI));

            using Pen curvePen = new Pen(Color.OrangeRed, 2f);
            PointF? prevPoint = null;

            int curveSteps = 300;
            for (int step = 0; step <= curveSteps; step++)
            {
                double xVal = histogramMin + (histogramMax - histogramMin) * step / curveSteps;
                double exponent = -(xVal - mu) * (xVal - mu) / (2 * sigma * sigma);
                double fVal = (1.0 / (sigma * Math.Sqrt(2 * Math.PI))) * Math.Exp(exponent);

                float px = marginLeft + (float)((xVal - histogramMin) / (histogramMax - histogramMin) * drawW);
                float py = marginTop + drawH - (float)(fVal / fPeak * drawH);

                PointF currentPoint = new PointF(px, py);

                if (prevPoint.HasValue)
                    g.DrawLine(curvePen, prevPoint.Value, currentPoint);

                prevPoint = currentPoint;
            }

            using Font titleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using StringFormat sfCenter = new StringFormat();
            sfCenter.Alignment = StringAlignment.Center;

            g.DrawString(
                $"Гистограмма  μ={mu:F3}  σ={sigma:F3}",
                titleFont,
                Brushes.DimGray,
                marginLeft + drawW / 2f,
                1,
                sfCenter);
        }
    }
}

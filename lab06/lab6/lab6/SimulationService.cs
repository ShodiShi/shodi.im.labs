using System;

namespace lab6
{
    public static class SimulationService
    {
        public static DiscreteSimulationResult RunDiscrete(int n, double[] xValues, double[] pValues)
        {
            int[] counts = new int[xValues.Length];

            for (int i = 0; i < n; i++)
            {
                double alpha = RandomGenerator.NextDouble();
                double a = alpha;

                for (int k = 0; k < pValues.Length; k++)
                {
                    a -= pValues[k];
                    if (a <= 0)
                    {
                        counts[k]++;
                        break;
                    }
                }
            }

            double theorAvg = 0;
            for (int i = 0; i < xValues.Length; i++)
                theorAvg += xValues[i] * pValues[i];

            double theorVar = 0;
            for (int i = 0; i < xValues.Length; i++)
                theorVar += xValues[i] * xValues[i] * pValues[i];
            theorVar -= theorAvg * theorAvg;

            double empAvg = 0;
            for (int i = 0; i < xValues.Length; i++)
                empAvg += xValues[i] * counts[i];
            empAvg /= n;

            double empVar = 0;
            for (int i = 0; i < xValues.Length; i++)
                empVar += xValues[i] * xValues[i] * counts[i];
            empVar = empVar / n - empAvg * empAvg;

            double errAvg = Math.Abs(theorAvg - empAvg) / theorAvg * 100;
            double errVar = Math.Abs(theorVar - empVar) / theorVar * 100;

            double chi2 = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                double expected = n * pValues[i];
                double diff = counts[i] - expected;
                chi2 += (diff * diff) / expected;
            }

            return new DiscreteSimulationResult
            {
                Counts = counts,
                TheoreticalAverage = theorAvg,
                EmpiricalAverage = empAvg,
                TheoreticalVariance = theorVar,
                EmpiricalVariance = empVar,
                AverageError = errAvg,
                VarianceError = errVar,
                ChiSquare = chi2
            };
        }

        public static NormalSimulationResult RunNormal(int n, double theorMu, double theorSigma, int numBins = 20)
        {
            double[] sample = new double[n];

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < 12; j++)
                    sum += RandomGenerator.NextDouble();

                sample[i] = theorMu + theorSigma * (sum - 6.0);
            }

            double mu = 0;
            for (int i = 0; i < n; i++)
                mu += sample[i];
            mu /= n;

            double variance = 0;
            for (int i = 0; i < n; i++)
            {
                double diff = sample[i] - mu;
                variance += diff * diff;
            }
            variance /= n;

            double sigma = Math.Sqrt(variance);

            double errMu = Math.Abs(theorMu - mu) / theorMu * 100;
            double errSigma = Math.Abs(theorSigma - sigma) / theorSigma * 100;

            double histogramMin = sample[0];
            double histogramMax = sample[0];

            for (int i = 1; i < n; i++)
            {
                if (sample[i] < histogramMin) histogramMin = sample[i];
                if (sample[i] > histogramMax) histogramMax = sample[i];
            }

            int[] histogramData = new int[numBins];
            double binWidth = (histogramMax - histogramMin) / numBins;

            for (int i = 0; i < n; i++)
            {
                int binIndex = (int)((sample[i] - histogramMin) / binWidth);
                if (binIndex >= numBins)
                    binIndex = numBins - 1;

                histogramData[binIndex]++;
            }

            return new NormalSimulationResult
            {
                Sample = sample,
                Mu = mu,
                Sigma = sigma,
                MuError = errMu,
                SigmaError = errSigma,
                HistogramData = histogramData,
                HistogramMin = histogramMin,
                HistogramMax = histogramMax
            };
        }
    }
}

using System;

namespace lab6
{
    public class DiscreteSimulationResult
    {
        public int[] Counts { get; set; } = Array.Empty<int>();
        public double TheoreticalAverage { get; set; }
        public double EmpiricalAverage { get; set; }
        public double TheoreticalVariance { get; set; }
        public double EmpiricalVariance { get; set; }
        public double AverageError { get; set; }
        public double VarianceError { get; set; }
        public double ChiSquare { get; set; }
    }

    public class NormalSimulationResult
    {
        public double[] Sample { get; set; } = Array.Empty<double>();
        public double Mu { get; set; }
        public double Sigma { get; set; }
        public double MuError { get; set; }
        public double SigmaError { get; set; }
        public int[] HistogramData { get; set; } = Array.Empty<int>();
        public double HistogramMin { get; set; }
        public double HistogramMax { get; set; }
    }
}

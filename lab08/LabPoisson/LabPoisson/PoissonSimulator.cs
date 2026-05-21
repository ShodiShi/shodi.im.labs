using System;
using System.Collections.Generic;

namespace LabPoisson
{
    public class PoissonSimulator
    {
        private readonly Random random = new Random();

        private double GenerateExponentialInterval(double lambda)
        {
            double r = random.NextDouble();
            return -Math.Log(1 - r) / lambda;
        }

        private int SimulateRequestsCount(double lambda, double T)
        {
            double currentTime = 0.0;
            int requestsCount = 0;

            while (true)
            {
                double interval = GenerateExponentialInterval(lambda);
                currentTime += interval;

                if (currentTime > T)
                {
                    break;
                }

                requestsCount++;
            }

            return requestsCount;
        }

        public List<int> RunExperiments(double lambda, double T, int experimentsCount)
        {
            List<int> requestsHistory = new List<int>();

            for (int i = 0; i < experimentsCount; i++)
            {
                int requestsCount = SimulateRequestsCount(lambda, T);
                requestsHistory.Add(requestsCount);
            }

            return requestsHistory;
        }

        public double[] BuildEmpiricalDistribution(List<int> requestsHistory)
        {
            if (requestsHistory.Count == 0)
            {
                return Array.Empty<double>();
            }

            int maxValue = 0;

            for (int i = 0; i < requestsHistory.Count; i++)
            {
                if (requestsHistory[i] > maxValue)
                {
                    maxValue = requestsHistory[i];
                }
            }

            double[] probabilities = new double[maxValue + 1];

            for (int i = 0; i < requestsHistory.Count; i++)
            {
                int value = requestsHistory[i];
                probabilities[value]++;
            }

            for (int k = 0; k < probabilities.Length; k++)
            {
                probabilities[k] /= requestsHistory.Count;
            }

            return probabilities;
        }

        public double CalculateMean(List<int> requestsHistory)
        {
            double sum = 0.0;

            for (int i = 0; i < requestsHistory.Count; i++)
            {
                sum += requestsHistory[i];
            }

            return sum / requestsHistory.Count;
        }

        public double CalculateVariance(List<int> requestsHistory, double mean)
        {
            double sum = 0.0;

            for (int i = 0; i < requestsHistory.Count; i++)
            {
                double difference = requestsHistory[i] - mean;
                sum += difference * difference;
            }

            return sum / requestsHistory.Count;
        }

        private double Factorial(int n)
        {
            double result = 1.0;

            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }

            return result;
        }

        public double[] BuildTheoreticalDistribution(double lambda, double T, int maxValue)
        {
            double a = lambda * T;
            double[] probabilities = new double[maxValue + 1];

            for (int k = 0; k <= maxValue; k++)
            {
                probabilities[k] = Math.Pow(a, k) * Math.Exp(-a) / Factorial(k);
            }

            return probabilities;
        }

        public SimulationResult RunSimulation(double lambda, double T, int experimentsCount)
        {
            List<int> requestsHistory = RunExperiments(lambda, T, experimentsCount);
            double empiricalMean = CalculateMean(requestsHistory);
            double empiricalVariance = CalculateVariance(requestsHistory, empiricalMean);
            double[] empiricalDistribution = BuildEmpiricalDistribution(requestsHistory);
            int maxValue = empiricalDistribution.Length - 1;
            double[] theoreticalDistribution = BuildTheoreticalDistribution(lambda, T, maxValue);

            return new SimulationResult
            {
                RequestsHistory = requestsHistory,
                EmpiricalDistribution = empiricalDistribution,
                TheoreticalDistribution = theoreticalDistribution,
                EmpiricalMean = empiricalMean,
                EmpiricalVariance = empiricalVariance,
                TheoreticalMean = lambda * T,
                TheoreticalVariance = lambda * T
            };
        }
    }

    public class SimulationResult
    {
        public List<int> RequestsHistory { get; set; } = new List<int>();
        public double[] EmpiricalDistribution { get; set; } = Array.Empty<double>();
        public double[] TheoreticalDistribution { get; set; } = Array.Empty<double>();
        public double EmpiricalMean { get; set; }
        public double EmpiricalVariance { get; set; }
        public double TheoreticalMean { get; set; }
        public double TheoreticalVariance { get; set; }
    }
}
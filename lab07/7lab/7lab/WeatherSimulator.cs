using System;
using System.Collections.Generic;

namespace lab7
{
    public class WeatherSimulator
    {
        private readonly Random _random = new Random();

        public double[,] Q { get; } = new double[3, 3];
        public int CurrentState { get; private set; }
        public int InitialState { get; private set; }
        public double CurrentTime { get; private set; }
        public double TotalTime { get; private set; }

        public List<(double time, int state)> History { get; } = new List<(double time, int state)>();
        public double[] StateTime { get; } = new double[3];

        public void SetRates(double q12, double q13, double q21, double q23, double q31, double q32)
        {
            Q[0, 1] = q12;
            Q[0, 2] = q13;
            Q[0, 0] = -(q12 + q13);

            Q[1, 0] = q21;
            Q[1, 2] = q23;
            Q[1, 1] = -(q21 + q23);

            Q[2, 0] = q31;
            Q[2, 1] = q32;
            Q[2, 2] = -(q31 + q32);
        }

        public void Start(double totalTime)
        {
            TotalTime = totalTime;
            CurrentTime = 0;
            InitialState = ChooseInitialState();
            CurrentState = InitialState;

            History.Clear();
            Array.Clear(StateTime, 0, StateTime.Length);
        }

        public bool Step()
        {
            if (CurrentTime >= TotalTime)
            {
                return false;
            }

            int stateBeforeTransition = CurrentState;
            var (nextState, dt) = NextState(stateBeforeTransition);

            if (CurrentTime + dt >= TotalTime)
            {
                dt = TotalTime - CurrentTime;
                StateTime[stateBeforeTransition] += dt;
                CurrentTime = TotalTime;
                History.Add((CurrentTime, stateBeforeTransition));
                return false;
            }

            StateTime[stateBeforeTransition] += dt;
            CurrentTime += dt;
            History.Add((CurrentTime, stateBeforeTransition));
            CurrentState = nextState;

            return true;
        }

        public int GetCurrentDay()
        {
            if (CurrentTime >= TotalTime && TotalTime > 0)
            {
                return (int)Math.Ceiling(TotalTime);
            }

            return (int)Math.Floor(CurrentTime) + 1;
        }

        private int ChooseInitialState()
        {
            int best = 0;
            double maxIntensity = -Q[0, 0];

            for (int i = 1; i < 3; i++)
            {
                double intensity = -Q[i, i];

                if (intensity > maxIntensity)
                {
                    maxIntensity = intensity;
                    best = i;
                }
            }

            return best;
        }

        private (int nextState, double dt) NextState(int state)
        {
            double qii = Q[state, state];
            double u = _random.NextDouble();

            if (u == 0)
            {
                u = 0.000001;
            }

            double dt = Math.Log(u) / qii;
            double r = _random.NextDouble();
            double sum = 0;

            for (int j = 0; j < 3; j++)
            {
                if (j == state)
                {
                    continue;
                }

                sum += Q[state, j] / (-qii);

                if (r < sum)
                {
                    return (j, dt);
                }
            }

            return (state, dt);
        }
    }
}

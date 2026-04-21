using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSimulations.Domain
{
    public class SimulationConfig
    {
        public int GridRows { get; init; } = 1000;
        public int GridColumns { get; init; } = 1000;
        public int TotalDays { get; init; } = 365;

        public double InfectionProbability { get; init; } = 0.3;

        public double RecoveryProbability { get; init; } = 0.05;

        public double DeathProbability { get; init; } = 0.01;

        public double InitialInfectedFraction { get; init; } = 0.001;

        public int RandomSeed { get; init; } = 42;
    }
}

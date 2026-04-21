using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSimulations.Domain
{
    public class DayStatistics
    {
        public int Day { get; init; }
        public long SusceptibleCount { get; init; }
        public long InfectedCount { get; init; }
        public long RecoveredCount { get; init; }
        public long DeadCount { get; init; }

        public long TotalPopulation => SusceptibleCount + InfectedCount + RecoveredCount + DeadCount;

        public double ReproductionNumber { get; init; }
    }
}

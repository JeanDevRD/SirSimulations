namespace SirSimulations.Domain
{
    public class SimulationResult
    {
        public int SimulationIndex { get; init; }
        public int RandomSeed { get; init; }
        public long PeakInfected { get; init; }
        public int PeakDay { get; init; }
        public int EpidemicDuration { get; init; }
        public long TotalDead { get; init; }
        public long TotalRecovered { get; init; }
        public long TotalPopulation { get; init; }

        public static SimulationResult FromHistory(int index, int seed, List<DayStatistics> history)
        {
            long peakInfected = 0;
            int peakDay = 0;
            int duration = 0;

            foreach (var day in history)
            {
                if (day.InfectedCount > peakInfected)
                {
                    peakInfected = day.InfectedCount;
                    peakDay = day.Day;
                }
                if (day.InfectedCount > 0)
                    duration = day.Day;
            }

            var last = history[^1];

            return new SimulationResult
            {
                SimulationIndex = index,
                RandomSeed = seed,
                PeakInfected = peakInfected,
                PeakDay = peakDay,
                EpidemicDuration = duration,
                TotalDead = last.DeadCount,
                TotalRecovered = last.RecoveredCount,
                TotalPopulation = last.TotalPopulation
            };
        }
    }
}

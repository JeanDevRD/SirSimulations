namespace SirSimulations.Domain
{
    public class MonteCarloSummary
    {
        public int SimulationCount { get; init; }

        public double AvgPeakInfected { get; init; }
        public long MinPeakInfected { get; init; }
        public long MaxPeakInfected { get; init; }

        public double AvgPeakDay { get; init; }
        public int MinPeakDay { get; init; }
        public int MaxPeakDay { get; init; }

        public double AvgDuration { get; init; }
        public int MinDuration { get; init; }
        public int MaxDuration { get; init; }

        public double AvgTotalDead { get; init; }
        public long MinTotalDead { get; init; }
        public long MaxTotalDead { get; init; }

        public static MonteCarloSummary From(List<SimulationResult> results)
        {
            return new MonteCarloSummary
            {
                SimulationCount = results.Count,
                AvgPeakInfected = results.Average(r => r.PeakInfected),
                MinPeakInfected = results.Min(r => r.PeakInfected),
                MaxPeakInfected = results.Max(r => r.PeakInfected),
                AvgPeakDay = results.Average(r => r.PeakDay),
                MinPeakDay = results.Min(r => r.PeakDay),
                MaxPeakDay = results.Max(r => r.PeakDay),
                AvgDuration = results.Average(r => r.EpidemicDuration),
                MinDuration = results.Min(r => r.EpidemicDuration),
                MaxDuration = results.Max(r => r.EpidemicDuration),
                AvgTotalDead = results.Average(r => r.TotalDead),
                MinTotalDead = results.Min(r => r.TotalDead),
                MaxTotalDead = results.Max(r => r.TotalDead)
            };
        }
    }
}
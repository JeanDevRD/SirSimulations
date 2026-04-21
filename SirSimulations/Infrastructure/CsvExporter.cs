using SirSimulations.Domain;

namespace SirSimulations.Infrastructure
{
    public class CsvExporter
    {
        public static void ExportSimulationResults(IEnumerable<SimulationResult> results, string filePath)
        {
            var lines = new List<string>
            {
                "SimIndex,Seed,PeakInfected,PeakDay,Duration,TotalDead,TotalRecovered,TotalPopulation"
            };

            foreach (var r in results)
                lines.Add($"{r.SimulationIndex},{r.RandomSeed},{r.PeakInfected}," +
                          $"{r.PeakDay},{r.EpidemicDuration},{r.TotalDead}," +
                          $"{r.TotalRecovered},{r.TotalPopulation}");

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"[CSV] Resultados guardados en: {filePath}");
        }

        public static void ExportSummary(MonteCarloSummary summary, string filePath)
        {
            var lines = new List<string>
            {
                "Metric,Avg,Min,Max",
                $"PeakInfected,{summary.AvgPeakInfected:F0},{summary.MinPeakInfected},{summary.MaxPeakInfected}",
                $"PeakDay,{summary.AvgPeakDay:F1},{summary.MinPeakDay},{summary.MaxPeakDay}",
                $"Duration,{summary.AvgDuration:F1},{summary.MinDuration},{summary.MaxDuration}",
                $"TotalDead,{summary.AvgTotalDead:F0},{summary.MinTotalDead},{summary.MaxTotalDead}"
            };

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"[CSV] Resumen guardado en: {filePath}");
        }

        public static void ExportScaling(
            double sequentialSeconds,
            List<(int threads, double seconds, double speedup)> results,
            string filePath)
        {
            var lines = new List<string> { "Threads,Time_s,Speedup,Efficiency" };
            lines.Add($"1_seq,{sequentialSeconds:F4},1.0000,1.0000");

            foreach (var (threads, seconds, speedup) in results)
            {
                double efficiency = speedup / threads;
                lines.Add($"{threads},{seconds:F4},{speedup:F4},{efficiency:F4}");
            }

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"[CSV] Scaling guardado en: {filePath}");
        }
    }
}
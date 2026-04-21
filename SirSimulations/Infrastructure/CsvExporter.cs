using SirSimulations.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSimulations.Infrastructure
{
    public class CsvExporter
    {
        public static void Export(IEnumerable<DayStatistics> history, string filePath)
        {
            var lines = new List<string>
        {
            "Day,Susceptible,Infected,Recovered,Dead,TotalPopulation"
        };

            foreach (var day in history)
                lines.Add($"{day.Day},{day.SusceptibleCount},{day.InfectedCount}," +
                          $"{day.RecoveredCount},{day.DeadCount},{day.TotalPopulation}");

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"[CSV] Resultados guardados en: {filePath}");
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

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
            var lines = new List<string> { "Day,Susceptible,Infected,Recovered,Dead,TotalPopulation" };

            foreach (var day in history)
                lines.Add($"{day.Day},{day.SusceptibleCount},{day.InfectedCount}," +
                          $"{day.RecoveredCount},{day.DeadCount},{day.TotalPopulation}");

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"[CSV] Resultados guardados en: {filePath}");
        }
    }
}

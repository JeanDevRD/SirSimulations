using SirSimulations.Application;
using SirSimulations.Domain;
using SirSimulations.Infrastructure;
using System.Diagnostics;

var config = new SimulationConfig
{
    GridRows = 1000,
    GridColumns = 1000,
    TotalDays = 365,
    InfectionProbability = 0.3,
    RecoveryProbability = 0.05,
    DeathProbability = 0.01,
    InitialInfectedFraction = 0.001,
    RandomSeed = 42
};

Console.WriteLine("Simulación SIR — Secuencial");
Console.WriteLine($"Grilla: {config.GridRows}×{config.GridColumns} | Días: {config.TotalDays}");
Console.WriteLine("Iniciando...\n");

var simulator = new SirSimulator(config);
simulator.InitializeGrid();

var stopwatch = Stopwatch.StartNew();
var history = simulator.Run();
stopwatch.Stop();

Console.WriteLine($"Simulación completada en {stopwatch.Elapsed.TotalSeconds:F2}s\n");

foreach (var day in history.Where(d => d.Day % 30 == 0 || d.Day == 1))
    Console.WriteLine($"Día {day.Day,3}: " +
                      $"S={day.SusceptibleCount,9:N0}  " +
                      $"I={day.InfectedCount,7:N0}  " +
                      $"R={day.RecoveredCount,7:N0}  " +
                      $"D={day.DeadCount,6:N0}");

CsvExporter.Export(history, "sequential_results.csv");
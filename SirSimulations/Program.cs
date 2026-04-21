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

Console.WriteLine("  Simulación SIR — Secuencial:");

var sequentialSimulator = new SirSimulator(config);
sequentialSimulator.InitializeGrid();

var sw = Stopwatch.StartNew();
var sequentialHistory = sequentialSimulator.Run();
sw.Stop();
double sequentialSeconds = sw.Elapsed.TotalSeconds;

Console.WriteLine($"Tiempo: {sequentialSeconds:F2}s\n");
CsvExporter.Export(sequentialHistory, "sequential_results.csv");

int[] threadCounts = { 1, 2, 4, 8 };
var scalingResults = new List<(int threads, double seconds, double speedup)>();

foreach (int threads in threadCounts)
{
    Console.WriteLine($"  Simulación SIR — Paralela ({threads} hilosL)");

    var parallelSimulator = new ParallelSirSimulator(config, threads);
    parallelSimulator.InitializeGrid();

    sw.Restart();
    var parallelHistory = parallelSimulator.Run();
    sw.Stop();
    double parallelSeconds = sw.Elapsed.TotalSeconds;

    double speedup = sequentialSeconds / parallelSeconds;
    scalingResults.Add((threads, parallelSeconds, speedup));

    Console.WriteLine($"Tiempo:   {parallelSeconds:F2}s");
    Console.WriteLine($"Speed-up: {speedup:F2}x\n");

    CsvExporter.Export(parallelHistory, $"parallel_results_{threads}threads.csv");
}

Console.WriteLine("  Strong Scaling — Resumen");

Console.WriteLine($"{"Hilos",-8} {"Tiempo (s)",-14} {"Speed-up",-10}");
Console.WriteLine(new string('-', 34));
Console.WriteLine($"{"Seq",-8} {sequentialSeconds,-14:F2} {"1.00x",-10}");

foreach (var (threads, seconds, speedup) in scalingResults)
    Console.WriteLine($"{threads,-8} {seconds,-14:F2} {speedup:F2}x");

CsvExporter.ExportScaling(sequentialSeconds, scalingResults, "scaling_results.csv");
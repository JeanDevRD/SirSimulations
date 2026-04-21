
using SirSimulations.Application.Parallels;
using SirSimulations.Domain;
using SirSimulations.Infrastructure;
using System.Diagnostics;

var config = new SimulationConfig
{
    GridRows = 100,
    GridColumns = 100,
    TotalDays = 365,
    InfectionProbability = 0.3,
    RecoveryProbability = 0.05,
    DeathProbability = 0.01,
    InitialInfectedFraction = 0.001,
    RandomSeed = 42
};

int simulationCount = 20;

Console.WriteLine("Monte Carlo SIR — Secuencial (1 hilo):");

var sw = Stopwatch.StartNew();
var sequentialRunner = new MonteCarloRunner(config, simulationCount, threadCount: 1);
var sequentialResults = sequentialRunner.Run();
sw.Stop();
double sequentialSeconds = sw.Elapsed.TotalSeconds;

Console.WriteLine($"Tiempo: {sequentialSeconds:F2}s");

var sequentialSummary = MonteCarloSummary.From(sequentialResults);
CsvExporter.ExportSimulationResults(sequentialResults, "sequential_results.csv");
CsvExporter.ExportSummary(sequentialSummary, "sequential_summary.csv");

Console.WriteLine($"Pico promedio de infectados: {sequentialSummary.AvgPeakInfected:F0}");
Console.WriteLine($"Duración promedio:           {sequentialSummary.AvgDuration:F1} días");
Console.WriteLine($"Muertes promedio:            {sequentialSummary.AvgTotalDead:F0}\n");

int[] threadCounts = { 1, 2, 4, 8 };
var scalingResults = new List<(int threads, double seconds, double speedup)>();

foreach (int threads in threadCounts)
{
    Console.WriteLine($"Monte Carlo SIR — Paralelo ({threads} hilos):");

    sw.Restart();
    var parallelRunner = new MonteCarloRunner(config, simulationCount, threadCount: threads);
    var parallelResults = parallelRunner.Run();
    sw.Stop();
    double parallelSeconds = sw.Elapsed.TotalSeconds;

    double speedup = sequentialSeconds / parallelSeconds;
    scalingResults.Add((threads, parallelSeconds, speedup));

    var summary = MonteCarloSummary.From(parallelResults);
    CsvExporter.ExportSimulationResults(parallelResults, $"parallel_results_{threads}threads.csv");
    CsvExporter.ExportSummary(summary, $"parallel_summary_{threads}threads.csv");

    Console.WriteLine($"Tiempo:   {parallelSeconds:F2}s");
    Console.WriteLine($"Speed-up: {speedup:F2}x\n");
}

Console.WriteLine("Strong Scaling — Resumen");
Console.WriteLine($"{"Hilos",-8} {"Tiempo (s)",-14} {"Speed-up",-10} {"Eficiencia",-10}");
Console.WriteLine(new string('-', 44));
Console.WriteLine($"{"Seq",-8} {sequentialSeconds,-14:F2} {"1.00x",-10} {"100%",-10}");

foreach (var (threads, seconds, speedup) in scalingResults)
{
    double efficiency = speedup / threads * 100;
    Console.WriteLine($"{threads,-8} {seconds,-14:F2} {speedup:F2}x      {efficiency:F0}%");
}

CsvExporter.ExportScaling(sequentialSeconds, scalingResults, "scaling_results.csv");
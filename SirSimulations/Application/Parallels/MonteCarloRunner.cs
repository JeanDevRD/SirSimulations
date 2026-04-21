using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SirSimulations.Domain;


namespace SirSimulations.Application.Parallels
{
    public class MonteCarloRunner
    {
        private readonly SimulationConfig _baseConfig;
        private readonly int _simulationCount;
        private readonly int _threadCount;

        public MonteCarloRunner(SimulationConfig baseConfig, int simulationCount, int threadCount)
        {
            _baseConfig = baseConfig;
            _simulationCount = simulationCount;
            _threadCount = threadCount;
        }

        public List<SimulationResult> Run()
        {
            var results = new SimulationResult[_simulationCount];

            var options = new ParallelOptions { MaxDegreeOfParallelism = _threadCount };

            Parallel.For(0, _simulationCount, options, i =>
            {
                var config = new SimulationConfig
                {
                    GridRows = _baseConfig.GridRows,
                    GridColumns = _baseConfig.GridColumns,
                    TotalDays = _baseConfig.TotalDays,
                    InfectionProbability = _baseConfig.InfectionProbability,
                    RecoveryProbability = _baseConfig.RecoveryProbability,
                    DeathProbability = _baseConfig.DeathProbability,
                    InitialInfectedFraction = _baseConfig.InitialInfectedFraction,
                    RandomSeed = _baseConfig.RandomSeed + i
                };

                var simulator = new SirSimulator(config);
                simulator.InitializeGrid();
                var history = simulator.Run();

                results[i] = SimulationResult.FromHistory(i, config.RandomSeed, history);
            });

            return results.ToList();
        }
    }
}
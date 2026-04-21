using SirSimulations.Application.Parallels;
using SirSimulations.Domain;
using SirSimulations.Domain.Enums;

namespace SirSimulations.Application
{
    public class SirSimulator
    {
        private readonly SimulationConfig _config;
        private readonly Grid _grid;
        private readonly Random _random;
        private long _previousSusceptible = 0;

        public SirSimulator(SimulationConfig config)
        {
            _config = config;
            _grid = new Grid(config.GridRows, config.GridColumns);
            _random = new Random(config.RandomSeed);
        }

        public void InitializeGrid()
        {
            for (int row = 0; row < _config.GridRows; row++)
                for (int col = 0; col < _config.GridColumns; col++)
                {
                    bool startsInfected = _random.NextDouble() < _config.InitialInfectedFraction;
                    _grid.SetNextCell(row, col,
                        startsInfected ? CellState.Infected : CellState.Susceptible);
                }
            _grid.SwapBuffers();
        }

        public List<DayStatistics> Run()
        {
            var history = new List<DayStatistics>(_config.TotalDays);

            for (int day = 1; day <= _config.TotalDays; day++)
            {
                UpdateAllCells();
                _grid.SwapBuffers();
                history.Add(CollectStatistics(day));
            }

            return history;
        }

        private void UpdateAllCells()
        {
            for (int row = 0; row < _config.GridRows; row++)
                for (int col = 0; col < _config.GridColumns; col++)
                    _grid.SetNextCell(row, col, ComputeNextState(row, col));
        }

        private CellState ComputeNextState(int row, int col)
        {
            CellState today = _grid.GetCell(row, col);
            return today switch
            {
                CellState.Susceptible => TryToGetInfected(row, col),
                CellState.Infected => TryToRecoverOrDie(),
                _ => today
            };
        }

        private CellState TryToGetInfected(int row, int col)
        {
            int infectedNeighbors = CountInfectedNeighbors(row, col);
            double probabilityOfStayingHealthy =
                Math.Pow(1.0 - _config.InfectionProbability, infectedNeighbors);
            return _random.NextDouble() > probabilityOfStayingHealthy
                ? CellState.Infected
                : CellState.Susceptible;
        }

        private CellState TryToRecoverOrDie()
        {
            if (_random.NextDouble() < _config.RecoveryProbability) return CellState.Recovered;
            if (_random.NextDouble() < _config.DeathProbability) return CellState.Dead;
            return CellState.Infected;
        }

        private int CountInfectedNeighbors(int row, int col)
        {
            int count = 0;
            for (int deltaRow = -1; deltaRow <= 1; deltaRow++)
                for (int deltaCol = -1; deltaCol <= 1; deltaCol++)
                {
                    if (deltaRow == 0 && deltaCol == 0) continue;
                    int neighborRow = row + deltaRow;
                    int neighborCol = col + deltaCol;
                    bool isInsideGrid = neighborRow >= 0 && neighborRow < _config.GridRows
                                     && neighborCol >= 0 && neighborCol < _config.GridColumns;
                    if (isInsideGrid && _grid.GetCell(neighborRow, neighborCol) == CellState.Infected)
                        count++;
                }
            return count;
        }

        private DayStatistics CollectStatistics(int day)
        {
            long susceptible = 0, infected = 0, recovered = 0, dead = 0;

            for (int row = 0; row < _config.GridRows; row++)
                for (int col = 0; col < _config.GridColumns; col++)
                    switch (_grid.GetCell(row, col))
                    {
                        case CellState.Susceptible: susceptible++; break;
                        case CellState.Infected: infected++; break;
                        case CellState.Recovered: recovered++; break;
                        case CellState.Dead: dead++; break;
                    }

            long newInfections = _previousSusceptible == 0 ? 0 : _previousSusceptible - susceptible;
            double r0 = infected > 0 ? (double)newInfections / infected : 0;
            _previousSusceptible = susceptible;

            return new DayStatistics
            {
                Day = day,
                SusceptibleCount = susceptible,
                InfectedCount = infected,
                RecoveredCount = recovered,
                DeadCount = dead,
                ReproductionNumber = r0
            };
        }
    }
}
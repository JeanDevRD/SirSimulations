using SirSimulations.Domain;
using SirSimulations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSimulations.Application
{
    public class ParallelSirSimulator
    {
        private readonly SimulationConfig _config;
        private readonly Grid _grid;
        private readonly int _threadCount;
        private readonly Random[] _randomPerThread;

        public ParallelSirSimulator(SimulationConfig config, int threadCount)
        {
            _config = config;
            _grid = new Grid(config.GridRows, config.GridColumns);
            _threadCount = threadCount;

            _randomPerThread = new Random[threadCount];
            for (int i = 0; i < threadCount; i++)
                _randomPerThread[i] = new Random(config.RandomSeed + i);
        }

        public void InitializeGrid()
        {
            var random = new Random(_config.RandomSeed);

            for (int row = 0; row < _config.GridRows; row++)
                for (int col = 0; col < _config.GridColumns; col++)
                {
                    bool startsInfected = random.NextDouble() < _config.InitialInfectedFraction;
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
                UpdateAllCellsInParallel();
                _grid.SwapBuffers();
                history.Add(CollectStatisticsInParallel(day));
            }

            return history;
        }

        private void UpdateAllCellsInParallel()
        {
            Parallel.For(0, _threadCount, threadIndex =>
            {
                (int firstRow, int lastRow) = CalculateRowRange(threadIndex);

                int ghostRowAbove = firstRow - 1;
                int ghostRowBelow = lastRow + 1;

                Random random = _randomPerThread[threadIndex];

                for (int row = firstRow; row <= lastRow; row++)
                    for (int col = 0; col < _config.GridColumns; col++)
                        _grid.SetNextCell(row, col,
                            ComputeNextState(row, col, ghostRowAbove, ghostRowBelow, random));
            });
        }

        private (int firstRow, int lastRow) CalculateRowRange(int threadIndex)
        {
            int rowsPerThread = _config.GridRows / _threadCount;
            int extraRows = _config.GridRows % _threadCount;

            int firstRow = threadIndex * rowsPerThread + Math.Min(threadIndex, extraRows);
            int lastRow = firstRow + rowsPerThread - 1 + (threadIndex < extraRows ? 1 : 0);

            return (firstRow, lastRow);
        }

        private CellState ComputeNextState(
            int row, int col,
            int ghostRowAbove, int ghostRowBelow,
            Random random)
        {
            CellState today = _grid.GetCell(row, col);

            return today switch
            {
                CellState.Susceptible => TryToGetInfected(row, col, ghostRowAbove, ghostRowBelow, random),
                CellState.Infected => TryToRecoverOrDie(random),
                _ => today
            };
        }

        private CellState TryToGetInfected(
            int row, int col,
            int ghostRowAbove, int ghostRowBelow,
            Random random)
        {
            int infectedNeighbors = 0;

            for (int deltaRow = -1; deltaRow <= 1; deltaRow++)
            {
                for (int deltaCol = -1; deltaCol <= 1; deltaCol++)
                {
                    if (deltaRow == 0 && deltaCol == 0) continue;

                    int neighborRow = row + deltaRow;
                    int neighborCol = col + deltaCol;

                    bool outsideGrid = neighborRow < 0
                                    || neighborRow >= _config.GridRows
                                    || neighborCol < 0
                                    || neighborCol >= _config.GridColumns;
                    if (outsideGrid) continue;

                    if (_grid.GetCell(neighborRow, neighborCol) == CellState.Infected)
                        infectedNeighbors++;
                }
            }

            double probabilityOfStayingHealthy =
                Math.Pow(1.0 - _config.InfectionProbability, infectedNeighbors);

            return random.NextDouble() > probabilityOfStayingHealthy
                ? CellState.Infected
                : CellState.Susceptible;
        }

        private CellState TryToRecoverOrDie(Random random)
        {
            if (random.NextDouble() < _config.RecoveryProbability) return CellState.Recovered;
            if (random.NextDouble() < _config.DeathProbability) return CellState.Dead;
            return CellState.Infected;
        }

        private DayStatistics CollectStatisticsInParallel(int day)
        {
            long totalSusceptible = 0, totalInfected = 0, totalRecovered = 0, totalDead = 0;

            Parallel.For(0, _threadCount, threadIndex =>
            {
                (int firstRow, int lastRow) = CalculateRowRange(threadIndex);

                long localSusceptible = 0, localInfected = 0, localRecovered = 0, localDead = 0;

                for (int row = firstRow; row <= lastRow; row++)
                    for (int col = 0; col < _config.GridColumns; col++)
                        switch (_grid.GetCell(row, col))
                        {
                            case CellState.Susceptible: localSusceptible++; break;
                            case CellState.Infected: localInfected++; break;
                            case CellState.Recovered: localRecovered++; break;
                            case CellState.Dead: localDead++; break;
                        }

                Interlocked.Add(ref totalSusceptible, localSusceptible);
                Interlocked.Add(ref totalInfected, localInfected);
                Interlocked.Add(ref totalRecovered, localRecovered);
                Interlocked.Add(ref totalDead, localDead);
            });

            return new DayStatistics
            {
                Day = day,
                SusceptibleCount = totalSusceptible,
                InfectedCount = totalInfected,
                RecoveredCount = totalRecovered,
                DeadCount = totalDead
            };
        }
    }
}

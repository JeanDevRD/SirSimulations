using SirSimulations.Domain.Enums;

namespace SirSimulations.Application.Parallels
{
    public class Grid
    {
        private CellState[,] _currentBuffer;
        private CellState[,] _nextBuffer;

        public int Rows { get; }
        public int Columns { get; }

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _currentBuffer = new CellState[rows, columns];
            _nextBuffer = new CellState[rows, columns];
        }

        public CellState GetCell(int row, int col) => _currentBuffer[row, col];

        public void SetNextCell(int row, int col, CellState state) => _nextBuffer[row, col] = state;

        public void SwapBuffers() => (_currentBuffer, _nextBuffer) = (_nextBuffer, _currentBuffer);

        public CellState[,] CurrentState => _currentBuffer;
    }
}

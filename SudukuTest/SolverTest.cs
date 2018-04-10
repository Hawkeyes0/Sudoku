using Suduku.Solve;
using Xunit;

namespace SudukuTest
{
    public class SolverTest
    {
        [Fact]
        public void TestRun()
        {
            Solver solver = new Solver();
            solver.Run("D:\\Codes\\dotnet\\Core\\Sudoku\\SudukuTest\\puzzle.txt");
        }
    }
}

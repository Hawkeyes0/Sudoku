using System;
using Suduku.Solve;
using Xunit;

namespace SudukuTest
{
    public class CellTest
    {
        [Fact]
        public void ToStringTest()
        {
            Cell cell = new Cell(){
                Value = 5
            };
            cell.Possible.AddRange(new int[]{3, 4});
            Assert.Equal("Value: 5, Possible: [3,4]", cell.ToString());
            cell.Possible.Clear();
            Assert.Equal("Value: 5, Possible: []", cell.ToString());
        }
    }
}

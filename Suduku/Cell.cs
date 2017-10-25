using System;
using System.Collections.Generic;
using System.Text;

namespace Suduku.Solve
{
    public class Cell
    {
        public int? Value { get; set; }

        public List<int> Possible { get; } = new List<int>();

        public Cell(char v)
        {
            if (v >= '0' && v <= '9')
            {
                Value = v - '0';
            }
        }

        public Cell()
        {
        }

        public override string ToString()
        {
            return $"Value: {Value}, Possible: {OutputPossible()}";
        }

        private string OutputPossible()
        {
            StringBuilder sb = new StringBuilder();
            foreach (int v in Possible)
            {
                sb.Append($"{v},");
            }
            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return $"[{sb}]";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Suduku.Solve
{
    /// 矩阵中的元素
    public class Cell
    {
        /// 元素的值。如果为空则未赋值
        public int? Value { get; set; }

        /// 元素的可能性。如果元素已经赋值，则不存在可能性；如果元素未赋值，且不存在可能性，则表示之前的计算有错误。
        public List<int> Possible { get; } = new List<int>();

        /// 构造方法。
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Suduku.Solve
{
    internal class Solver
    {
        private Cell[,] _matrix = new Cell[9, 9];
        private Cell[,] _previou = null;
        private Cell[,] _blocks = new Cell[9, 9];
        private int _loopCounter = 1;
        private Stack<Cell[,]> _branchs = new Stack<Cell[,]>();

        /// 输入题目<br/>
        /// 尝试解题<br/>
        /// 解不下去就找备选答案最少的单元格，并创建分支，继续解题，直到得到答案为止。
        internal void Run(string file = null)
        {
            InputMatrix(file);
            DateTime start = DateTime.Now;
            do
            {
                SolveMatrix();
                if (IsWrong() && _branchs.Count > 1 || CannotContinue())
                {
                    _blocks = _branchs.Pop();
                    _matrix = _branchs.Pop();
                    Debug.WriteLine($"Switch to other branch {_branchs.Count}.");
                }
                else if (Finished())
                {
                    break;
                }
                else
                {
                    PushBranches();
                    Debug.WriteLine($"Create branches {_branchs.Count}.");
                }
                _loopCounter++;
            } while (!Finished());
            TimeSpan span = DateTime.Now - start;
            PrintMatrix();
            Console.WriteLine($"Used time: {span.TotalMilliseconds}ms.");
        }

        /// 遍历所有元素，如果存在既没有赋值，又不存在可能性的元素，则表明当前矩阵无法继续解算
        private bool CannotContinue()
        {
            foreach (Cell cell in _matrix)
            {
                if (!cell.Value.HasValue && cell.Possible.Count == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// 遍历所有元素，搜索可能性最少的元素，基于该元素建立可能性分支，并保留最后一种可能性，把其他可能性压栈
        private void PushBranches()
        {
            Cell cell = null;
            foreach (Cell c in _matrix)
            {
                if (cell == null && !c.Value.HasValue)
                {
                    cell = c;
                }
                else if (!c.Value.HasValue && cell.Possible.Count > c.Possible.Count)
                {
                    cell = c;
                }
            }

            foreach (int num in cell.Possible)
            {
                cell.Value = num;
                var clone = CloneFrom(_matrix);
                _branchs.Push(clone.Matrix);
                _branchs.Push(clone.Blocks);
            }

            if (cell.Possible.Count > 1)
            {
                _branchs.Pop();
                _branchs.Pop();
            }
        }

        /// 判断是否所有元素都被赋值
        private bool Finished()
        {
            int count = 0;
            foreach (Cell cell in _matrix)
            {
                // if (!cell.Value.HasValue && cell.Possible.Count == 0)
                // {
                //     return true;
                // }
                if (cell.Value.HasValue)
                {
                    count++;
                }
            }
            return count == 81;
        }

        /// 复制输入的矩阵
        private (Cell[,] Matrix, Cell[,] Blocks) CloneFrom(Cell[,] from)
        {
            Cell[,] toMatrix = new Cell[from.GetLength(0), from.GetLength(1)];
            Cell[,] toBlocks = new Cell[from.GetLength(0), from.GetLength(1)];
            int i;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (toMatrix[r, c] == null)
                    {
                        toMatrix[r, c] = new Cell { Value = from[r, c].Value };
                    }
                }
            }
            for (int b = 0; b < 9; b++)
            {
                i = 0;
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        toBlocks[b, i] = toMatrix[((int)(b / 3)) * 3 + r, (b % 3) * 3 + c];
                        i++;
                    }
                }
            }

            return (toMatrix, toBlocks);
        }

        /// 打印矩阵
        private void PrintMatrix()
        {
            Console.WriteLine($"Loop {_loopCounter}:");
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    Console.Write($"{(_matrix[r, c].Value.HasValue ? _matrix[r, c].Value.ToString() : "-")} ");
                }
                Console.WriteLine();
            }
            _loopCounter++;
        }

        /// 解算问题矩阵
        private void SolveMatrix()
        {
            ComputePossible();
            FillNumber();
        }

        /// 计算矩阵中每个元素的可能性
        private void ComputePossible()
        {
            ComputeRow();
            ComputeColumn();
            ComputeBlock();
        }

        /// 按区块进行可能性排除
        private void ComputeBlock()
        {
            List<int> numbers = new List<int>();
            for (int b = 0; b < 9; b++)
            {
                for (int i = 0; i < 9; i++)
                {
                    numbers = _blocks[b, i].Possible;
                    for (int ip = 0; ip < 9; ip++)
                    {
                        if (ip == i) continue;
                        if (_blocks[b, ip].Value.HasValue)
                        {
                            numbers.Remove(_blocks[b, ip].Value.Value);
                        }
                    }
                }
            }
        }

        /// 按列进行可能性排除
        private void ComputeColumn()
        {
            List<int> numbers;
            for (int c = 0; c < 9; c++)
            {
                for (int r = 0; r < 9; r++)
                {
                    numbers = _matrix[r, c].Possible;
                    for (int rp = 0; rp < 9; rp++)
                    {
                        if (r == rp) continue;
                        if (_matrix[rp, c].Value.HasValue)
                        {
                            numbers.Remove(_matrix[rp, c].Value.Value);
                        }
                    }
                }
            }
        }

        /// 按行进行可能性排除
        private void ComputeRow()
        {
            List<int> numbers;
            for (int r = 0; r < 9; r++)
            {
                numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                for (int c = 0; c < 9; c++)
                {
                    if (_matrix[r, c].Value.HasValue)
                    {
                        numbers.Remove(_matrix[r, c].Value.Value);
                    }
                }
                for (int c = 0; c < 9; c++)
                {
                    if (_matrix[r, c].Value.HasValue)
                        continue;
                    _matrix[r, c].Possible.Clear();
                    _matrix[r, c].Possible.AddRange(numbers);
                }
            }
        }

        /// 扫描仅存在唯一可能性的元素，并为其赋值
        private void FillNumber()
        {
            FillCell();
            FillRow();
            FillColumn();
            FillBlock();
        }

        /// 扫描每个元素，如果某元素仅存在唯一的可能性，则将其赋值为该可能的值
        private void FillCell()
        {
            foreach (var cell in _matrix)
            {
                if (cell.Value.HasValue) continue;
                if (cell.Possible.Count == 1)
                {
                    cell.Value = cell.Possible[0];
                    cell.Possible.Clear();
                    ComputePossible();
                }
            }
        }

        /// 扫描每个元素的可能性，如果某一可能性在其元素所在行中唯一，则将该元素赋值为该可能性值
        private void FillRow()
        {
            Dictionary<int, int> counter = new Dictionary<int, int>();
            for (int r = 0; r < 9; r++)
            {
                ResetCounter(counter);
                for (int c = 0; c < 9; c++)
                {
                    Cell cell = _matrix[r, c];
                    if (cell.Value.HasValue) continue;
                    foreach (int num in cell.Possible)
                    {
                        for (int cp = 0; cp < 9; cp++)
                        {
                            if (cp == c) continue;
                            if (_matrix[r, cp].Possible.Contains(num))
                            {
                                counter[num]++;
                            }
                        }
                    }
                    if (counter.ContainsValue(0))
                    {
                        foreach (var entry in counter)
                        {
                            if (cell.Possible.Contains(entry.Key))
                            {
                                if (entry.Value == 0)
                                {
                                    cell.Value = entry.Key;
                                    cell.Possible.Clear();
                                    ComputePossible();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// 重置可能性搜索计数器
        private void ResetCounter(Dictionary<int, int> counter)
        {
            for (int i = 1; i < 10; i++)
            {
                counter[i] = 0;
            }
        }

        /// 扫描每个元素的可能性，如果某一可能性在其元素所在列中唯一，则将该元素赋值为该可能性值
        private void FillColumn()
        {
            Dictionary<int, int> counter = new Dictionary<int, int>();
            for (int c = 0; c < 9; c++)
            {
                ResetCounter(counter);
                for (int r = 0; r < 9; r++)
                {
                    Cell cell = _matrix[r, c];
                    if (cell.Value.HasValue) continue;
                    foreach (int num in cell.Possible)
                    {
                        for (int rp = 0; rp < 9; rp++)
                        {
                            if (rp == r) continue;
                            if (_matrix[rp, c].Possible.Contains(num))
                            {
                                counter[num]++;
                            }
                        }
                    }
                    if (counter.ContainsValue(0))
                    {
                        foreach (var entry in counter)
                        {
                            if (cell.Possible.Contains(entry.Key))
                            {
                                if (entry.Value == 0)
                                {
                                    cell.Value = entry.Key;
                                    cell.Possible.Clear();
                                    ComputePossible();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// 扫描每个元素的可能性，如果某一可能性在其元素所在区块中唯一，则将该元素赋值为该可能性值
        private void FillBlock()
        {
            Dictionary<int, int> counter = new Dictionary<int, int>();
            for (int b = 0; b < 9; b++)
            {
                ResetCounter(counter);
                for (int i = 0; i < 9; i++)
                {
                    Cell cell = _blocks[b, i];
                    if (cell.Value.HasValue) continue;
                    foreach (int num in cell.Possible)
                    {
                        for (int ip = 0; ip < 9; ip++)
                        {
                            if (ip == i) continue;
                            if (_blocks[b, ip].Possible.Contains(num))
                            {
                                counter[num]++;
                            }
                        }
                    }
                    if (counter.ContainsValue(0))
                    {
                        foreach (var entry in counter)
                        {
                            if (cell.Possible.Contains(entry.Key))
                            {
                                if (entry.Value == 0)
                                {
                                    cell.Value = entry.Key;
                                    cell.Possible.Clear();
                                    ComputePossible();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// 判断矩阵中已赋值的元素是否符合规则
        private bool IsWrong()
        {
            return IsRowWrong() || IsColumnWrong() || IsBlockWrong();
        }

        /// 遍历每个元素，查看该元素是否在其所在区块中唯一
        private bool IsBlockWrong()
        {
            List<int> numbers = new List<int>();
            for (int b = 0; b < 9; b++)
            {
                for (int i = 0; i < 9; i++)
                {
                    Cell cell = _blocks[b, i];
                    if (cell.Value.HasValue)
                    {
                        if (numbers.Contains(cell.Value.Value))
                        {
                            return true;
                        }
                        numbers.Add(cell.Value.Value);
                    }
                }
                numbers.Clear();
            }
            return false;
        }

        /// 遍历每个元素，查看该元素是否在其所在列中唯一
        private bool IsColumnWrong()
        {
            List<int> numbers = new List<int>();
            for (int c = 0; c < 9; c++)
            {
                for (int r = 0; r < 9; r++)
                {
                    Cell cell = _matrix[r, c];
                    if (cell.Value.HasValue)
                    {
                        if (numbers.Contains(cell.Value.Value))
                        {
                            return true;
                        }
                        numbers.Add(cell.Value.Value);
                    }
                }
                numbers.Clear();
            }
            return false;
        }

        /// 遍历每个元素，查看该元素是否在其所在行中唯一
        private bool IsRowWrong()
        {
            List<int> numbers = new List<int>();
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    Cell cell = _matrix[r, c];
                    if (cell.Value.HasValue)
                    {
                        if (numbers.Contains(cell.Value.Value))
                        {
                            return true;
                        }
                        numbers.Add(cell.Value.Value);
                    }
                }
                numbers.Clear();
            }
            return false;
        }

        /// 输入数独矩阵的每一行，每个单元格连续输入，空白单元格用空格代替。然后根据输入的内容生成数独矩阵和区块矩阵
        private void InputMatrix(string file)
        {
            int i = 0;
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                Console.WriteLine("Please input the sudoku matrix. Don't split each number. Replace blank with SPACE.");
                for (i = 0; i < 9; i++)
                {
                    do
                    {
                        Console.WriteLine($"Row {i + 1}:");
                    } while (!FillMatrix(Console.ReadLine(), i));
                }
            }
            else
            {
                string[] lines = File.ReadAllLines(file);
                for (i = 0; i < 9; i++)
                {
                    FillMatrix(lines[i], i);
                }
            }

            List<int> numbers = new List<int>();
            for (int b = 0; b < 9; b++)
            {
                i = 0;
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        _blocks[b, i] = _matrix[((int)(b / 3)) * 3 + r, (b % 3) * 3 + c];
                        i++;
                    }
                }
            }
        }

        /// 根据输入的字符串和行索引号，填充数独矩阵
        private bool FillMatrix(string input, int rownum)
        {
            if (input.Length != 9)
            {
                Console.WriteLine("String lenth must be 9!");
                return false;
            }
            for (int i = 0; i < 9; i++)
            {
                _matrix[rownum, i] = new Cell(input[i]);
            }
            return true;
        }
    }
}
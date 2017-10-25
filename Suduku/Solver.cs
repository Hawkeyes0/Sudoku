using System;
using System.Collections.Generic;

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
        internal void Run()
        {
            InputMatrix();
            do
            {
                SolveMatrix();
                if (IsWrong() && _branchs.Count > 1 || CannotContinue())
                {
                    _blocks = _branchs.Pop();
                    _matrix = _branchs.Pop();
                    Console.WriteLine($"Switch to other branch {_branchs.Count}.");
                }
                else if (Finished())
                {
                    break;
                }
                else
                {
                    PushBranches();
                    Console.WriteLine($"Create branches {_branchs.Count}.");
                }
                _loopCounter++;
            } while (!Finished());
            PrintMatrix();
        }

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

        private bool EqualsPreviou()
        {
            if (_previou == null)
            {
                _previou = new Cell[9, 9];
                _previou = CloneFrom(_matrix).Matrix;
                return false;
            }
            bool rs = true;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    rs &= _previou[r, c].Value == _matrix[r, c].Value;
                }
            }
            if (!rs)
            {
                _previou = CloneFrom(_matrix).Matrix;
            }
            return rs;
        }

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

        private void SolveMatrix()
        {
            ComputePossible();
            FillNumber();
        }

        private void ComputePossible()
        {
            ComputeRow();
            ComputeColumn();
            ComputeBlock();
        }

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

        private void FillNumber()
        {
            FillCell();
            FillRow();
            FillColumn();
            FillBlock();
        }

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

        private void ResetCounter(Dictionary<int, int> counter)
        {
            for (int i = 1; i < 10; i++)
            {
                counter[i] = 0;
            }
        }

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

        private bool IsWrong()
        {
            return IsRowWrong() || IsColumnWrong() || IsBlockWrong();
        }

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

        private void InputMatrix()
        {
            int i = 0;
            Console.WriteLine("Please input the sudoku matrix. Don't split each number. Replace blank with SPACE.");
            for (i = 0; i < 9; i++)
            {
                do
                {
                    Console.WriteLine($"Row {i + 1}:");
                } while (!FillMatrix(Console.ReadLine(), i));
            }
            // FillMatrix("3 21  5 9", 0);
            // FillMatrix("         ", 1);
            // FillMatrix(" 4     8 ", 2);
            // FillMatrix("7   361  ", 3);
            // FillMatrix(" 6     3 ", 4);
            // FillMatrix("    28   ", 5);
            // FillMatrix("5     7  ", 6);
            // FillMatrix("  6     4", 7);
            // FillMatrix("  89    1", 8);
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
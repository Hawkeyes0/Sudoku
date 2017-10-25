using System;
using Suduku.Solve;

namespace Suduku
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Solver solver = new Solver();
            solver.Run();
        }
    }
}

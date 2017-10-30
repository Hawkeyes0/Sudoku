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
            if (args.Length > 1)
            {
                if (args[0] == "--file")
                {
                    solver.Run(args[1]);
                }
                else
                {
                    solver.Run();
                }
            }
        }
    }
}

using System.Collections.Generic;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var variant = args.Length == 0 || !int.TryParse(args[0], out var v)
                ? 12
                : v;
            var (c, d, a, x, b, bz) = GetData(variant);
            
            Printer.PrintSolution(Executor.Perform(c, d, a, x, b, bz));
        }

        static (double[] c, double[,] d, double[,] a, double[] x, ISet<int> b, ISet<int> bz) GetData(int variant)
        {
            double[] c = null;
            double[,] d = null;
            double[,] a = null;
            double[] x = null;
            ISet<int> b = null;
            ISet<int> bz = null;

            if (variant == 12)
            {
                c = new double[] {-1, -1, -1};
                d = new double[,] {{8, -4, 0}, {-4, 4, 0}, {0, 0, 2}};
                a = new double[,] {{2, 4, 0}, {1, 0, 1}};
                x = new double[] {0, 1, 1};
                b = new HashSet<int> {1, 2};
                bz = new HashSet<int> {1, 2};
            }
            
            else if (variant == 8)
            {
                c = new double[] {-1, -1, -1};
                d = new double[,] {{2, -1, 0}, {-1, 2, 0}, {0, 0, 2}};
                a = new double[,] {{1, 1, 0}, {1, 0, 2}};
                x = new double[] {0, 2, 1};
                b = new HashSet<int> {1, 2};
                bz = new HashSet<int> {1, 2};
            }
            
            else if (variant == 1)
            {
                c = new double[] {-1, 0, 0};
                d = new double[,] {{4, -2, 0}, {-2, 4, 0}, {0, 0, 1}};
                a = new double[,] {{6, 6, 0}, {3, 0, 1}};
                x = new double[] {0, 0.5, 1};
                b = new HashSet<int> {1, 2};
                bz = new HashSet<int> {1, 2};
            }
            
            return (c, d, a, x, b, bz);
        }
    }
}

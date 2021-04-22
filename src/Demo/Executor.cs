using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Demo
{
    public class Executor
    {
        private readonly double[] _c;
        private readonly double[,] _D;
        private readonly double[,] _A;
        private readonly double[] _x;
        private readonly ISet<int> _B;
        private readonly ISet<int> _Bz;

        public Executor(double[] c, double[,] d, double[,] a, double[] x, ISet<int> b, ISet<int> bz)
        {
            _c = c;
            _D = d;
            _A = a;
            _x = x;
            _B = b;
            _Bz = bz;
        }

        public QuadraticResult Perform()
        {
            return QuadraticOptimizationService.Solve(_c, _D, _A, _x, _B, _Bz);
        }

        public static QuadraticResult Perform(
            double[] c, 
            double[,] d, 
            double[,] a, 
            double[] x, 
            ISet<int> b,
            ISet<int> bz)
        {
            var executor = new Executor(c, d, a, x, b, bz);
            return executor.Perform();
        }
    }
}

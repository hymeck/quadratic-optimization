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

        public Executor(
            double[] c, 
            double[,] D, 
            double[,] A, 
            double[] x, 
            ISet<int> B,
            ISet<int> Bz)
        {
            _c = c;
            _D = D;
            _A = A;
            _x = x;
            _B = B;
            _Bz = Bz;
        }

        public QuadraticResult Perform()
        {
            return QuadraticOptimizationService.Solve(_c, _D, _A, _x, _B, _Bz);
        }

        public static QuadraticResult Perform(
            double[] c, 
            double[,] D, 
            double[,] A, 
            double[] x, 
            ISet<int> B,
            ISet<int> Bz)
        {
            var executor = new Executor(c, D, A, x, B, Bz);
            return executor.Perform();
        }
    }
}

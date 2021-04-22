using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
// ReSharper disable InconsistentNaming

namespace Demo
{
    public class QuadraticOptimizationService
    {
        private readonly double[] _c;
        private readonly double[,] _D;
        private readonly double[,] _A;
        private readonly double[] _x;
        private readonly ISet<int> _B;
        private readonly ISet<int> _Bz;

        public QuadraticOptimizationService(double[] c, double[,] D, double[,] A, double[] x, ISet<int> B, ISet<int> Bz)
        {
            _c = c;
            _D = D;
            _A = A;
            _x = x;
            _B = B;
            _Bz = Bz;
        }

        public QuadraticResult Solve()
        {
            var c = Vector<double>.Build.DenseOfArray(_c);
            var D = Matrix<double>.Build.DenseOfArray(_D);
            var A = Matrix<double>.Build.DenseOfArray(_A);
            var x = Vector<double>.Build.DenseOfArray(_x);

            return Core.Solve(c, D, A, x, _B.ToArray(), _Bz.ToArray());
        }

        public static QuadraticResult Solve(double[] c, double[,] D, double[,] A, double[] x, ISet<int> B, ISet<int> Bz)
        {
            var service = new QuadraticOptimizationService(c, D, A, x, B, Bz);
            return service.Solve();
        }
    }
}

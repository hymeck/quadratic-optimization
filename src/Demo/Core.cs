using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
// ReSharper disable InconsistentNaming

namespace Demo
{
    public static class Core
    {
        public static QuadraticResult Solve(Vector<double> c, Matrix<double> D, Matrix<double> A, Vector<double> x, ISet<int> B, ISet<int> Bz)
        {
            while (true)
            {
                var cx = c + D.Multiply(x);
                
                for (var i = 0; i < cx.Count; i++)
                    if (cx[i] < 0.000001) // less than 10^-6
                        cx[i] = 0;

                var cbx = B
                    .Select(basisItem => basisItem)
                    .Select(basisItem => -cx[basisItem]); // minus cx[basisItem] because it is used to build ux
                
            }
        }
    }
}
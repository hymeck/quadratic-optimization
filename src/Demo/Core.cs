using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
// ReSharper disable InconsistentNaming

namespace Demo
{
    public static class Core
    {
        public static QuadraticResult Solve(Vector<double> c, Matrix<double> D, Matrix<double> A, Vector<double> x, int[] Jb, int[] Jbz)
        {
            while (true)
            {
                Debug.WriteLine(D.ToMatrixString());
                var cx = c + D.Multiply(x);
                
                for (var i = 0; i < cx.Count; i++)
                    if (Math.Abs(cx[i]) < 0.000001) // less than 10^-6
                        cx[i] = 0;

                var cbx = Vector<double>.Build.DenseOfEnumerable(Jb
                    .Select(basisItem => -cx[basisItem])); // minus cx[basisItem] because it is used to build ux

                var inversedAb = Matrix<double>.Build.DenseOfColumns(Jbz
                        .Select(A.Column))
                    .Inverse();

                // var ux = inversedAb.Multiply(cbx);
                var ux = inversedAb.LeftMultiply(cbx);
                
                // var deltaX = A.Multiply(ux) + cx;
                Debug.WriteLine(A.ToMatrixString());
                var deltaX = A.LeftMultiply(ux) + cx;
                for (var i = 0; i < deltaX.Count; i++)
                    if (Math.Abs(deltaX[i]) < 0.000001) // less than 10^-6
                        deltaX[i] = 0;
                
                if (deltaX.Minimum() >= 0)
                    return QuadraticResult.Create(x.ToArray());

                var j0 = FindJ0(deltaX);

                var Jnb = Enumerable
                    .Range(0, A.ColumnCount)
                    .Except(Jbz)
                    .ToArray();

                var l = BuildL(A, Jnb, j0);

                var Abz = Matrix<double>.Build.DenseOfColumns(Jbz.Select(A.Column));
                
                var d = BuildMatrixFromListOfLists(BuildListOfLists(D, A, Jbz));

                var zeroMatrix = Matrix<double>.Build.Dense(2, 2, 0d); // 2x2 hardcoded value?

                var H = MakeMatrixFromBlocks(d, Abz, Abz.Transpose(), zeroMatrix);
                var inversedH = H.Inverse();
                Debug.WriteLine(H.ToMatrixString());

                var bz = Buildbz(D, A, Jbz, j0);

                var xx = inversedH.Negate().Multiply(bz);
                for (var i = 0; i < Jbz.Length; i++)
                    l[Jbz[i]] = xx[i];


                var sigma = D.LeftMultiply(l).DotProduct(l);

                var theta = Enumerable.Repeat(double.MaxValue, A.RowCount).ToArray();
                if (sigma != 0)
                    theta[j0] = Math.Abs(deltaX[j0]) / sigma;
                for (var i = 0; i < A.RowCount; i++)
                    if (Jbz.Contains(i) && l[i] < 0)
                        theta[i] = -(x[i] - l[i]);

                var min = double.MaxValue;
                var pi = 0;
                for (var i = 0; i < theta.Length; i++)
                    if (theta[i] < min)
                    {
                        min = theta[i];
                        pi = i;
                    }
                
                if (Math.Abs(min - double.MaxValue) < 0.000001) // less than 10^-6
                    return QuadraticResult.Empty;

                x += l.Multiply(min);
                
            }
        }

        private static int FindJ0(Vector<double> deltaX)
        {
            var j0 = 0;
            for (var i = 0; i < deltaX.Count; i++)
            {
                if (deltaX[i] < 0)
                {
                    j0 = i;
                    break;
                }
            }

            return j0;
        }

        private static Vector<double> BuildL(Matrix<double> A, int[] Jnb, int j0)
        {
            var l = Vector<double>.Build.Dense(A.ColumnCount, 0d);
            for (var i = 0; i < Jnb.Length; i++)
            {
                if (Jnb[i] == j0)
                    l[j0] = 1;
            }

            return l;
        }

        private static List<List<double>> BuildListOfLists(Matrix<double> D, Matrix<double> A, ICollection<int> Jbz)
        {
            var d = new List<List<double>>(A.RowCount);

            for (var i = 0; i < D.RowCount; i++)
            {
                d.Add(new List<double>(A.ColumnCount)); // `append()` in python
                for (var j = 0; j < D.ColumnCount; j++)
                    if (Jbz.Contains(i) && Jbz.Contains(j))
                        d[^1].Add(D[i, j]); // ^1 is like -1 in python
            }

            for (var i = 0; i < d.Count;)
            {
                if (d[i].Count == 0)
                    d.RemoveAt(i); // `pop()` in python
                else
                    i++;
            }

            return d;
        }

        private static Vector<double> Buildbz(Matrix<double> D, Matrix<double> A, ICollection<int> Jbz, int j0)
        {
            var _bz = new Queue<double>(Jbz.Count + A.RowCount);
            foreach (var i in Jbz)
                _bz.Enqueue(D[j0, i]);
            for (var i = 0; i < A.RowCount; i++)
                _bz.Enqueue(A[i, j0]);

            return Vector<double>.Build.DenseOfEnumerable(_bz);
        }

        private static Matrix<double> MakeMatrixFromBlocks(
            Matrix<double> upperLeft, Matrix<double> upperRight,
            Matrix<double> lowerLeft, Matrix<double> lowerRight)
        {
            // var upperLeft = Matrix<double>.Build.DenseOfArray(new double[,] {{1, 2}, {3, 4}});
            // var upperRight = Matrix<double>.Build.DenseOfArray(new double[,] {{5, 6}, {7, 8}});
            // var lowerLeft = Matrix<double>.Build.DenseOfArray(new double[,] {{9, 10}, {11, 12}});
            // var lowerRight = Matrix<double>.Build.DenseOfArray(new double[,] {{13, 14}, {15, 16}});
            // 1  2  5  6
            // 3  4  7  8
            // 9  10 13 14
            // 13 14 15 16
            return Matrix<double>.Build.DenseOfMatrixArray(new[,]
            {
                {upperLeft, upperRight}, 
                {lowerLeft, lowerRight}
            });
        }

        private static Matrix<double> BuildMatrixFromListOfLists(List<List<double>> d)
        {
            var _d = new double[d.Count, d[0].Count];
            for (var row = 0; row < d.Count; row++)
            {
                var r = d[row];
                for (var column = 0; column < r.Count; column++)
                    _d[row, column] = r[column];
            }

            return Matrix<double>.Build.DenseOfArray(_d);
        }
    }
}

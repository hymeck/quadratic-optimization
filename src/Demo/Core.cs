using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
// ReSharper disable InconsistentNaming

namespace Demo
{
    public static class Core
    {
        public static QuadraticResult Solve(Vector<double> c, Matrix<double> D, Matrix<double> A, Vector<double> x, List<int> Jb, List<int> Jbz)
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
                
                var Ab = Matrix<double>.Build.Dense(A.RowCount, Jb.Count);
                for (var i = 0; i < Jb.Count; i++)
                    Ab.SetColumn(i, A.Column(Jbz[i]));

                var inversedAb = Ab.Inverse();
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
                for (var i = 0; i < Jbz.Count; i++)
                    l[Jbz[i]] = xx[i];


                var sigma = D.LeftMultiply(l).DotProduct(l);

                var theta = Enumerable.Repeat(double.MaxValue, A.ColumnCount).ToArray();
                if (sigma != 0)
                    theta[j0] = Math.Abs(deltaX[j0]) / sigma;
                for (var i = 0; i < A.ColumnCount; i++)
                    if (Jbz.Contains(i))
                        if (l[i] < 0)
                            theta[i] = -(x[i] / l[i]);

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

                // f1 = np.setdiff1d(Jbz, Jb)
                var f1 = Jbz
                    .Except(Jb)
                    .ToList();

                if (pi == j0) // case 1
                    Jbz.Add(j0);
                else if (f1.Contains(pi)) // case 2
                    Jbz.Remove(pi); // or `RemoveAll()` is enough, idk
                else // case 3
                {
                    var fl = GetFl(A, Jb, Jbz, pi, f1, inversedAb);

                    if (fl != 0) 
                        continue;
                    
                    var s = Jb.IndexOf(pi); // returns -1 if there is no pi
                    if (s == -1)
                        continue;

                    // for i in f1:
                    //     q = np.dot(Ab_inv, a[:, i - 1])
                    var q = inversedAb.Multiply(A.Column(f1[^1]));

                    if (q[s] == 0)
                    {
                        for (var ii = 0; ii < Jb.Count; ii++)
                            if (Jb[ii] == pi)
                                Jb[ii] = j0;
                        
                        for (var ii = 0; ii < Jbz.Count; ii++)
                            if (Jbz[ii] == pi)
                                Jbz[ii] = j0;
                    }

                    if (Jb.SequenceEqual(Jbz)) // np.array_equal(Jb, Jbz)
                    {
                        for (var ii = 0; ii < Jb.Count; ii++)
                            if (Jb[ii] == pi)
                                Jb[ii] = j0;
                        
                        for (var ii = 0; ii < Jbz.Count; ii++)
                            if (Jbz[ii] == pi)
                                Jbz[ii] = j0;
                    }
                }
            }
        }

        private static int GetFl(Matrix<double> A, IList<int> Jb, ICollection<int> Jbz, int pi, List<int> f1, Matrix<double> inversedAb)
        {
            var fl = 0;

            var s = Jb.IndexOf(pi); // returns -1 if there is no pi

            if (s != -1)
            {
                for (var i = 0; i < f1.Count; i++)
                {
                    var item = f1[i];
                    var q = inversedAb.Multiply(A.Column(item));
                    if (q[s] != 0)
                    {
                        for (var ii = 0; ii < Jb.Count; ii++)
                        {
                            if (Jb[ii] == pi)
                                Jb[ii] = item;
                        }

                        Jbz.Remove(pi); // or `RemoveAll()` is enough, idk
                        fl = 1;
                        break;
                    }
                }
            }

            return fl;
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

using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Helpers
{
    public static class MatrixHelpers
    {
        public static void Rank1UpdateMultR_u0(double[,] A, double[] u, double u_0, double gamma, int colA0, int w0, int w1, double[] _temp)
        {
            //int indexA = 0;
            // reordered to reduce cpu cache issues
            for (int i = colA0; i < A.GetLength(1); i++)
            {
                _temp[i] = u_0 * A[w0, i];
            }

            for (int k = w0 + 1; k < w1; k++)
            {
                //indexA = k * A.GetLength(1) + colA0;
                double valU = u[k];
                for (int i = colA0; i < A.GetLength(1); i++)
                {
                    _temp[i] += valU * A[k, i];
                }
            }

            for (int i = colA0; i < A.GetLength(1); i++)
            {
                _temp[i] *= gamma;
            }


            //indexA = w0 * A.GetLength(1) + colA0;
            for (int j = colA0; j < A.GetLength(1); j++)
            {
                A[w0, j] -= u_0 * _temp[j];
            }

            for (int i = w0 + 1; i < w1; i++)
            {
                double valU = u[i];

                //indexA = i * A.GetLength(1) + colA0;
                for (int j = colA0; j < A.GetLength(1); j++)
                {
                    A[i, j] -= valU * _temp[j];
                }
            }
        }
    }
}

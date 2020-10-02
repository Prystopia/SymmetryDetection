//using SymmetryDetection.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Text;

//namespace SymmetryDetection.Optimisation
//{
//    public class QRSolverEigen
//    {
//        public enum InfoEnum
//        {
//            Success
//        }
//        private float[,] Matrix { get; set; }
//        public InfoEnum Info { get; set; }
//        public float[,] MatrixR => Matrix;

//        public bool[,] ColsPermutation { get; set; }
//        public float[,] MatrixQ { get; set
//        public int Rank { get; set; }
//        public int DetPQ { get; set; }
//        public QRSolver(float[,] jacobianMatric)
//        {
//            Matrix = jacobianMatric;
//            Solve();
//        }
//        private void Solve()
//        {
//            int rows = Matrix.GetLength(0);
//            int cols = Matrix.GetLength(1);
//            int size = rows * cols;

//            float[] hCoeffs = new float[size];
//            float[] temp = new float[cols];
//            float[] colsTranspositions = new float[cols];

//            int numTranspositions = 0;
//            float[] colNormsUpdated = new float[cols];
//            float[] colNormsDirect = new float[cols];

//            for (int i = 0; i < cols; i++)
//            {
//                colNormsDirect[i] = Matrix.GetColumn(i).Norm();
//                colNormsUpdated[i] = colNormsDirect[i];
//            }

//            float thresholdHelper = MathF.Abs(MathF.Pow((colNormsUpdated.MaxCoefficient(out _) * ExtensionMethods.EPSILON / rows), 2));
//            float normDowndateThreshold = MathF.Sqrt(ExtensionMethods.EPSILON);

//            float nonZeroPivots = size;
//            float maxPivot = 0;

//            for (int k = 0; k < size; k++)
//            {
//                // first, we look up in our table m_colNormsUpdated which column has the biggest norm
//                float biggestColSquaredNorm = MathF.Abs(MathF.Pow(colNormsUpdated.Tail(cols - k).MaxCoefficient(out int biggestColIndex), 2));
//                biggestColIndex += k;

//                // Track the number of meaningful pivots but do not stop the decomposition to make
//                // sure that the initial matrix is properly reproduced. See bug 941.
//                if (nonZeroPivots == size && biggestColSquaredNorm < thresholdHelper * (rows - k))
//                    nonZeroPivots = k;

//                // apply the transposition to the columns
//                colsTranspositions[k] = biggestColIndex;
//                if (k != biggestColIndex)
//                {
//                    Matrix.SwapCol(k, biggestColIndex);

//                    colNormsUpdated[k] = colNormsUpdated[biggestColIndex];
//                    colNormsDirect[k] = colNormsDirect[biggestColIndex];

//                    ++numTranspositions;
//                }

//                // generate the householder vector, store it below the diagonal
//                Matrix.GetColumn(k).Tail(rows - k).MakeHouseholderInPlace(hCoeffs[k], out float beta);

//                // apply the householder transformation to the diagonal coefficient
//                Matrix[k, k] = beta;

//                // remember the maximum absolute value of diagonal coefficients
//                if (MathF.Abs(beta) > maxPivot)
//                {
//                    maxPivot = MathF.Abs(beta);
//                }

//                // apply the householder transformation
//                Matrix.BottomRightCorner(rows - k, cols - k - 1).ApplyHouseholderOnTheLeft(Matrix.GetColumn(k).Tail(rows - k - 1), hCoeffs[k], out float tempVal);
//                temp[k + 1] = tempVal;

//                // update our table of norms of the columns
//                for (int j = k + 1; j < cols; ++j)
//                {
//                    // The following implements the stable norm downgrade step discussed in
//                    // http://www.netlib.org/lapack/lawnspdf/lawn176.pdf
//                    // and used in LAPACK routines xGEQPF and xGEQP3.
//                    // See lines 278-297 in http://www.netlib.org/lapack/explore-html/dc/df4/sgeqpf_8f_source.html
//                    if (colNormsUpdated[j] != 0)
//                    {
//                        float tempX = MathF.Abs(Matrix[k, j] / colNormsUpdated[j]);
//                        tempX = (1 + tempX) * (1 - tempX);
//                        tempX = tempX < 0 ? 0 : tempX;

//                        float temp2 = tempX * MathF.Abs(MathF.Pow(colNormsUpdated[j] / colNormsDirect[j], 2));
//                        if (temp2 <= normDowndateThreshold)
//                        {
//                            // The updated norm has become too inaccurate so re-compute the column
//                            // norm directly.
//                            colNormsDirect[j] = Matrix.GetColumn(j).Tail(rows - k - 1).Norm();
//                            colNormsUpdated[j] = colNormsDirect[j];
//                        }
//                        else
//                        {
//                            colNormsUpdated[j] *= MathF.Sqrt(tempX);
//                        }
//                    }
//                }
//            }

//            ColsPermutation = new bool[cols, cols];
//            for (int k = 0; k < size; ++k)
//            {
//                ColsPermutation.ApplyTranspositionOnTheRight(k, colsTranspositions[k]);
//            }

//            DetPQ = (numTranspositions % 2 == 0) ? -1 : 1;
//        }

        
//    }
//}

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SymmetryDetection.Extensions;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;

namespace SymmetryDetection.Optimisation
{
    public class QRDecomposerHouseHolder : IDecompositionHandler
    {
        //https://github.com/lessthanoptimal/ejml/blob/e838418796934dd61e9f63ccb57e652653841088/main/ejml-ddense/src/org/ejml/dense/row/decomposition/qr/QRDecompositionHouseholderColumn_DDRM.java#L37
        private double[,] A; // [ column][ row ]
        private double[,] R;
        private double[,] Q;

        // used internally to store temporary data
        protected double[] v;

        // dimension of the decomposed matrices
        protected int numCols; // this is 'n'
        protected int numRows; // this is 'm'
        protected int minLength;

        // the computed gamma for Q_k matrix
        private double[] gammas;
        // local variables
        private double gamma;
        private double tau;

        // did it encounter an error?
        private bool error;

        public void SetExpectedMaxSize(int numRows, int numCols)
        {
            this.numCols = numCols;
            this.numRows = numRows;
            minLength = Math.Min(numCols, numRows);
            int maxLength = Math.Max(numCols, numRows);

            //initialise matrices
            R = new double[numRows, numCols];

            Q = Identity(numRows, numRows);
            v = new double[maxLength];
            gammas = new double[minLength];
        }


        public double[,] GetQR()
        {
            return A;
        }

        public double[,] GetQ()
        {
            return Q;
        }

        private static double[,] Identity(int numRows, int numCols)
        {
            double[,] ret = new double[numRows, numCols];

            int small = numRows < numCols ? numRows : numCols;

            for (int i = 0; i < small; i++)
            {
                ret[i, i] = 1f;
            }

            return ret;
        }

        /**
         * Returns an upper triangular matrix which is the R in the QR decomposition.  If compact then the input
         * expected to be size = [min(rows,cols) , numCols] otherwise size = [numRows,numCols].
         *
         * @param R Storage for upper triangular matrix.
         * @param compact If true then a compact matrix is expected.
         */
        public double[,] GetR()
        {
            return R;
        }

        /**
         * <p>
         * To decompose the matrix 'A' it must have full rank.  'A' is a 'm' by 'n' matrix.
         * It requires about 2n*m<sup>2</sup>-2m<sup>2</sup>/3 flops.
         * </p>
         *
         * <p>
         * The matrix provided here can be of different
         * dimension than the one specified in the constructor.  It just has to be smaller than or equal
         * to it.
         * </p>
         */
        public bool Decompose(ref double[,] A)
        {
            SetExpectedMaxSize(A.GetLength(0), A.GetLength(1));

            this.A = this.R = A;

            error = false;

            for (int j = 0; j < minLength; j++)
            {
                HouseholderR(j);
                //UpdateA(j);
            }

            return error;
        }

        ///**
        // * Converts the standard row-major matrix into a column-major vector
        // * that is advantageous for this problem.
        // *
        // * @param A original matrix that is to be decomposed.
        // */
        //private void ConvertToColumnMajor(double[,] A)
        //{
        //    for (int x = 0; x < numCols; x++)
        //    {
        //        for (int y = 0; y < numRows; y++)
        //        {
        //            dataQR[x, y] = A[y, x];
        //        }
        //    }
        //}

        //private double[,] ConvertToRowMajor(double[,] QR)
        //{
        //    double[,] newQR = new double[QR.GetLength(1), QR.GetLength(0)];
        //    for (int x = 0; x < numRows; x++)
        //    {
        //        for (int y = 0; y < numCols; y++)
        //        {
        //            newQR[x, y] = QR[y, x];
        //        }
        //    }
        //    return newQR;
        //}

        /**
         * <p>
         * Computes the householder vector "u" for the first column of submatrix j.  Note this is
         * a specialized householder for this problem.  There is some protection against
         * overfloaw and underflow.
         * </p>
         * <p>
         * Q = I - &gamma;uu<sup>T</sup>
         * </p>
         * <p>
         * This function finds the values of 'u' and '&gamma;'.
         * </p>
         *
         * @param j Which submatrix to work off of.
         */
        protected void HouseholderR(int col)
        {
            //get Range of elements in column based on index - 
            double[] column = A.GetColumn(col).GetRange(col);

            //find the largest value in this column
            double max = column.Select(x => Math.Abs(x)).Max();

            if (max == 0.0)
            {
                gamma = 0;
                error = true;
            }
            else
            {

                var x = column.Divide(max);
                var norm = x.SquaredNorm();

                var u = x;

                if(u[0] >= 0)
                {
                    u[0] += norm;
                }
                else
                {
                    u[0] -= norm;
                }

                //transform u array into a single columned matrix to allow calculations and transposition
                var uTransposed = new double[1, column.Length];
                var uStandard = new double[column.Length, 1];
                for (int i = 0; i < column.Length; i++)
                {
                    uTransposed[0, i] = u[i];
                    uStandard[i, 0] = u[i];
                }

                double beta = 2 / (uTransposed.Multiply(uStandard)[0,0]);

                double[,] houseHolderReflectionCoefficient = uStandard.Multiply(beta).Multiply(uTransposed);

                //need to be careful we are only updating the sub-matrix of QR[startIndex:, col:]
                var subHouseholderMatrix = A.SubMatrix(col, A.GetLength(0), col, A.GetLength(1));
                subHouseholderMatrix = subHouseholderMatrix.Subtract(houseHolderReflectionCoefficient.Multiply(subHouseholderMatrix));

                //apply back to R matrix;
                for (int i = col; i < numRows; i++)
                {
                    for (int j = col; j < numCols; j++)
                    {
                        double val = subHouseholderMatrix[i - col, j - col];
                        if (val >= -ExtensionMethods.EPSILON && val <= ExtensionMethods.EPSILON)
                        {
                            val = 0;
                        }
                        R[i, j] = val;
                    }
                }
                HouseholderQ(col, uStandard, uTransposed);
            }

            gammas[col] = gamma;
        }

        private void HouseholderQ(int col, double[,] u, double[,] uTransposed)
        {
            double beta = 2 / Math.Pow(u.GetColumn(col).SquaredNorm(), 2);
            int m = numRows;
            var qSubMatrix = Q.SubMatrix(0, Q.GetLength(0), col, Q.GetLength(1));
            var qCoefficient = u.Multiply(uTransposed);

            qSubMatrix = qSubMatrix.Subtract(qCoefficient.Multiply(qSubMatrix).Multiply(beta));
            for (int i = 0; i < Q.GetLength(0); i++)
            {
                for (int j = col; j < Q.GetLength(1); j++)
                {
                    double val = qSubMatrix[i, j - col];
                    if (val >= -ExtensionMethods.EPSILON && val <= ExtensionMethods.EPSILON)
                    {
                        val = 0;
                    }
                    Q[i, j] = val;
                }
            }

        }

        private static void DivideElements(int startIndex, int endIndex, double[] u, double coefficient)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                u[i] /= coefficient;
            }
        }

        private static double ComputeTauAndDivide(int j, int numRows, double[] u, double max)
        {
            double tau = 0;
            for (int i = j; i < numRows; i++)
            {
                double d = u[i] /= max;
                tau += d * d;
            }
            tau = Math.Sqrt(tau);

            if (u[j] < 0)
                tau = -tau;

            return tau;
        }
       

        /**
         * <p>
         * Takes the results from the householder computation and updates the 'A' matrix.<br>
         * <br>
         * A = (I - &gamma;*u*u<sup>T</sup>)A
         * </p>
         *
         * @param w The submatrix.
         */
        //protected void UpdateA(int w)
        //{
        //    double[] u = dataQR.GetRow(w);

        //    for (int j = w + 1; j < numCols; j++)
        //    {

        //        double[] colQ = dataQR.GetRow(j);
        //        double val = colQ[w];

        //        for (int k = w + 1; k < numRows; k++)
        //        {
        //            val += u[k] * colQ[k];
        //        }
        //        val *= gamma;

        //        dataQR[j, w] -= val;
        //        for (int i = w + 1; i < numRows; i++)
        //        {
        //            dataQR[j, i] -= u[i] * val;
        //        }
        //    }
        //}

        public double[] GetGammas()
        {
            return gammas;
        }
    }
}

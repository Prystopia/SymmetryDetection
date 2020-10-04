using System;
namespace SymmetryDetection.Optimisation
{
    public class QRDecomposer
    {
        //https://github.com/lessthanoptimal/ejml/blob/e838418796934dd61e9f63ccb57e652653841088/main/ejml-ddense/src/org/ejml/dense/row/decomposition/qr/QRDecompositionHouseholderColumn_DDRM.java#L37
        protected float[][] dataQR; // [ column][ row ]

        // used internally to store temporary data
        protected float[] v;

        // dimension of the decomposed matrices
        protected int numCols; // this is 'n'
        protected int numRows; // this is 'm'
        protected int minLength;

        // the computed gamma for Q_k matrix
        protected float[] gammas;
        // local variables
        protected float gamma;
        protected float tau;

        // did it encounter an error?
        protected bool error;

        public void SetExpectedMaxSize(int numRows, int numCols)
        {
            this.numCols = numCols;
            this.numRows = numRows;
            minLength = Math.Min(numCols, numRows);
            int maxLength = Math.Max(numCols, numRows);

            if (dataQR == null || dataQR.GetLength(0) < numCols || dataQR[0].Length < numRows)
            {
                dataQR = new float[numCols][];

                for (int i = 0; i < numCols; i++)
                {
                    dataQR[i] = new float[numRows];
                }

                v = new float[maxLength];
                gammas = new float[minLength];
            }

            if (v.Length < maxLength)
            {
                v = new float[maxLength];
            }
            if (gammas.Length < minLength)
            {
                gammas = new float[minLength];
            }
        }

        /**
         * Returns the combined QR matrix in a 2D array format that is column major.
         *
         * @return The QR matrix in a 2D matrix column major format. [ column ][ row ]
         */
        public float[][] GetQR()
        {
            return dataQR;
        }

        /**
         * Returns an upper triangular matrix which is the R in the QR decomposition.  If compact then the input
         * expected to be size = [min(rows,cols) , numCols] otherwise size = [numRows,numCols].
         *
         * @param R Storage for upper triangular matrix.
         * @param compact If true then a compact matrix is expected.
         */
        public float[,] GetR(float[,] R, bool compact)
        {
            if (compact)
            {
                R = CheckZerosLT(R, minLength, numCols);
            }
            else
            {
                R = CheckZerosLT(R, numRows, numCols);
            }

            for (int j = 0; j < numCols; j++)
            {
                float[] colR = dataQR[j];
                int l = Math.Min(j, numRows - 1);
                for (int i = 0; i <= l; i++)
                {
                    float val = colR[i];
                    R[i, j] = val;
                }
            }

            return R;
        }

        private static float[,] CheckZerosLT(float[,] A, int numRows, int numCols)
        {
            if (A == null)
            {
                A = new float[numRows, numCols];
            }
            else if (numRows != A.GetLength(0) || numCols != A.GetLength(1))
            {
                A = new float[numRows, numCols]; 
            }

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    A[i, j] = 0;
                }
            }

            return A;
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
        public bool Decompose(float[,] A)
        {
            SetExpectedMaxSize(A.GetLength(0), A.GetLength(1));

            ConvertToColumnMajor(A);

            error = false;

            for (int j = 0; j < minLength; j++)
            {
                Householder(j);
                UpdateA(j);
            }

            return !error;
        }

        /**
         * Converts the standard row-major matrix into a column-major vector
         * that is advantageous for this problem.
         *
         * @param A original matrix that is to be decomposed.
         */
        protected void ConvertToColumnMajor(float[,] A)
        {
            for (int x = 0; x < numCols; x++)
            {
                float[] colQ = dataQR[x];
                for (int y = 0; y < numRows; y++)
                {
                    colQ[y] = A[y, x];
                }
            }
        }

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
        protected void Householder(int j)
        {
            float[] u = dataQR[j];

            // find the largest value in this column
            // this is used to normalize the column and mitigate overflow/underflow
            float max = FindMax(u, j, numRows - j);

            if (max == 0.0)
            {
                gamma = 0;
                error = true;
            }
            else
            {
                // computes tau and normalizes u by max
                tau = ComputeTauAndDivide(j, numRows, u, max);

                // divide u by u_0
                float u_0 = u[j] + tau;
                DivideElements(j + 1, numRows, u, u_0);

                gamma = u_0 / tau;
                tau *= max;

                u[j] = -tau;
            }

            gammas[j] = gamma;
        }

        private static void DivideElements(int j, int numRows, float[] u, float u_0)
        {
            for (int i = j; i < numRows; i++)
            {
                u[i] /= u_0;
            }
        }

        private static float ComputeTauAndDivide(int j, int numRows, float[] u, float max)
        {
            float tau = 0;
            for (int i = j; i < numRows; i++)
            {
                float d = u[i] /= max;
                tau += d * d;
            }
            tau = MathF.Sqrt(tau);

            if (u[j] < 0)
                tau = -tau;

            return tau;
        }

        private static float FindMax(float[] u, int startU, int length)
        {
            float max = -1;

            int index = startU;
            int stopIndex = startU + length;
            for (; index < stopIndex; index++)
            {
                float val = u[index];
                val = (val < 0.0) ? -val : val;
                if (val > max)
                    max = val;
            }

            return max;
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
        protected void UpdateA(int w)
        {
            float[] u = dataQR[w];

            for (int j = w + 1; j < numCols; j++)
            {

                float[] colQ = dataQR[j];
                float val = colQ[w];

                for (int k = w + 1; k < numRows; k++)
                {
                    val += u[k] * colQ[k];
                }
                val *= gamma;

                colQ[w] -= val;
                for (int i = w + 1; i < numRows; i++)
                {
                    colQ[i] -= u[i] * val;
                }
            }
        }

        public float[] GetGammas()
        {
            return gammas;
        }
    }
}

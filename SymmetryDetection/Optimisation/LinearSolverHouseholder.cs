using System;
using SymmetryDetection.Extensions;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
namespace SymmetryDetection.Optimisation
{
    public class LinearSolverHouseHolder : ILinearSolver
    {
        //https://github.com/lessthanoptimal/ejml/blob/e838418796934dd61e9f63ccb57e652653841088/main/ejml-ddense/src/org/ejml/dense/row/linsol/qr/LinearSolverQrHouseCol_DDRM.java#L48
        protected IDecompositionHandler decomposer;

        protected double[,] a = new double[1, 1];
        protected double[,] temp = new double[1, 1];

        protected int maxRows = -1;
        protected int maxCols = -1;

        protected double[,] QR; // a column major QR matrix
        protected double[,] R = new double[1, 1];
        protected double[] gammas;

        protected double[,] A;
        protected int numRows;
        protected int numCols;

        public LinearSolverHouseHolder(IDecompositionHandler decomposer)
        {
            this.decomposer = decomposer;
        }

        public void SetMaxSize(int maxRows, int maxCols)
        {
            this.maxRows = maxRows;
            this.maxCols = maxCols;
        }

        /**
         * Performs QR decomposition on A
         *
         * @param A not modified.
         */
         public bool SetA(double[,] A)
         {
            if (A.GetLength(0) > maxRows || A.GetLength(1) > maxCols)
            {
                SetMaxSize(A.GetLength(0), A.GetLength(1));
            }

            R = new double[A.GetLength(1), A.GetLength(1)];
            a = new double[A.GetLength(0), 1];
            temp = new double[A.GetLength(0), 1];

            this.A = A;
            this.numRows = A.GetLength(0);
            this.numCols = A.GetLength(1);


            if (decomposer.Decompose(ref A))
                return false;

            gammas = decomposer.GetGammas();
            QR = decomposer.GetQR();
            R = decomposer.GetR();
            return true;
        }

        /**
         * Solves for X using the QR decomposition.
         *
         * @param B A matrix that is n by m.  Not modified.
         * @param X An n by m matrix where the solution is written to.  Modified.
         */
        public double[,] Solve(double[,] B)
        {
            var X = new double[numCols, B.GetLength(1)];
            if (B.GetLength(0) != numRows)
            {
                throw new Exception("Unexpected dimensions for B: B rows = " + B.GetLength(0) + " expected = " + numRows);
            }

            int BnumCols = B.GetLength(1);

            // solve each column one by one
            for (int colB = 0; colB < BnumCols; colB++)
            {
                // make a copy of this column in the vector
                for (int i = 0; i < numRows; i++)
                {
                    a[i,0] = B[i, colB];
                }

                // Solve Qa=b
                // a = Q'b
                // a = Q_{n-1}...Q_2*Q_1*b
                //
                // Q_n*b = (I-gamma*u*u^T)*b = b - u*(gamma*U^T*b)
                for (int n = 0; n < numCols; n++)
                {
                    double[] u = QR.GetRow(n);
                    //MatrixHelpers.Rank1UpdateMultR_u0(a, u, 1f, gammas[n], 0, n, numRows, temp.To1DArray());
                }

                // solve for Rx = b using the standard upper triangular solver
                SolveU(R, a, numCols);

                // save the results
                for (int i = 0; i < numCols; i++)
                {
                    X[i, colB] = a[i,0];
                }
            }

            return X;
        }


        public static void SolveU(double[,] U, double[,] b, int n)
        {
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = b[i, 0];
                int indexU = i * n + i + 1;
                for (int j = i + 1; j < n; j++)
                {
                    sum -= U[i, j] * b[j, 0];
                }
                b[i, 0] = sum / U[i, i];
            }
        }
    }
}

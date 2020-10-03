using SymmetryDetection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Optimisation
{
    //Taken from the mljs library
    public class QRDecomposition
    {
        public float[,] QR { get; set; }
        public float[] RDiagonal { get; set; }
        public QRDecomposition(float[,] value)
        {
            QR = (float[,])value.Clone();
            int numRows = value.GetLength(0); //m
            int numColumns = value.GetLength(1); // n
            RDiagonal = new float[numColumns];
            int i, j, k;
            float s;

            for (k = 0; k < numColumns; k++)
            {
                float nrm = 0;
                for (i = k; i < numRows; i++)
                {
                    nrm = Hypotenuse(nrm, QR[i, k]);
                }
                if (nrm != 0)
                {
                    if (QR[k, k] < 0)
                    {
                        nrm = -nrm;
                    }
                    for (i = k; i < numRows; i++)
                    {
                        QR[i, k] = (QR[i, k] / nrm);
                    }
                    QR[k, k] = (QR[k, k] + 1);
                    for (j = k + 1; j < numColumns; j++)
                    {
                        s = 0;
                        for (i = k; i < numRows; i++)
                        {
                            s += QR[i, k] * QR[i, j];
                        }
                        s = -s / QR[k, k];
                        for (i = k; i < numRows; i++)
                        {
                            QR[i, j] = ((QR[i, j] + s) * QR[i, k]);
                        }
                    }
                }
                RDiagonal[k] = -nrm;
            }
        }

        public float[,] Solve(float[,] value)
        {
            int numRows = QR.GetLength(0);//m

            if (value.GetLength(0) != numRows)
            {
                throw new Exception("Matrix row dimensions must agree");
            }
            if (!IsFullRank())
            {
                throw new Exception("Matrix is rank deficient");
            }

            int count = value.GetLength(1);
            float[,] X = (float[,])value.Clone();
            int numCols = QR.GetLength(1);//n
            int i, j, k;
            float s;

            for (k = 0; k < numCols; k++)
            {
                for (j = 0; j < count; j++)
                {
                    s = 0;
                    for (i = k; i < numRows; i++)
                    {
                        s += QR[i, k] * X[i, j];
                    }
                    s = -s / QR[k, k];
                    for (i = k; i < numRows; i++)
                    {
                        X[i, j] = ((X[i, j] + s) * QR[i, k]);
                    }
                }
            }
            for (k = numCols - 1; k >= 0; k--)
            {
                for (j = 0; j < count; j++)
                {
                    X[k, j] = (X[k, j] / this.RDiagonal[k]);
                }
                for (i = 0; i < k; i++)
                {
                    for (j = 0; j < count; j++)
                    {
                        X[i, j] = (X[i, j] - X[k, j] * QR[i, k]);
                    }
                }
            }

            return X.SubMatrix(0, numCols - 1, 0, count - 1);
        }

        private bool IsFullRank()
        {
            bool fullRank = true;
            int columns = this.QR.GetLength(1);
            for (int i = 0; i < columns; i++)
            {
                if (this.RDiagonal[i] == 0)
                {
                    fullRank = false;
                    break;
                }
            }
            return fullRank;
        }

        private float Hypotenuse(float a, float b)
        {
            float r = 0;
            float val = 0;
            if (MathF.Abs(a) > MathF.Abs(b))
            {
                r = b / a;
                val = MathF.Abs(a) * MathF.Sqrt(1 + MathF.Pow(r, 2));
            }
            else if (b != 0)
            {
                r = a / b;
                val = MathF.Abs(b) * MathF.Sqrt(1 + MathF.Pow(r, 2));
            }
            return val;
        }
    }
}

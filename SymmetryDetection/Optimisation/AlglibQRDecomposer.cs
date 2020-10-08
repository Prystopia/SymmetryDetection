using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Optimisation
{
    public class AlglibQRDecomposer : IDecompositionHandler
    {
        double[] Gammas;
        double[,] R;
        double[,] Q;
        double[,] QR;
        double[,] A;
        public bool Decompose(ref double[,] A)
        {
            this.A = A;
            alglib.ortfac.rmatrixqr(ref A, A.GetLength(0), A.GetLength(1), ref Gammas, new alglib.xparams(0));
            QR = A;
            alglib.ortfac.rmatrixqrunpackr(A, A.GetLength(0), A.GetLength(1), ref R, new alglib.xparams(0));
            alglib.ortfac.rmatrixqrunpackq(A, A.GetLength(0), A.GetLength(1), Gammas, A.GetLength(0), ref Q, new alglib.xparams(0));
            return false;
        }

        public double[] GetGammas()
        {
            return Gammas;
        }

        public double[,] GetQ()
        {
            return Q;
        }

        public double[,] GetQR()
        {
            return QR;
        }

        public double[,] GetR()
        {
            return R;
        }
    }
}

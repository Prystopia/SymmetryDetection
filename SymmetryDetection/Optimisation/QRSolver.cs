using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Optimisation
{
    public class QRSolver
    {
        public enum InfoEnum
        {
            Success
        }
        private float[,] Matrix { get; set; }
        public InfoEnum Info { get; set; }
        public float[,] MatrixR { get; set; }

        public bool[,] ColsPermutation { get; set; }
        public float[,] MatrixQ { get; set; }
        public int Rank { get; set; }
        public QRSolver(float[,] jacobianMatric)
        {
            Matrix = jacobianMatric;
        }
    }
}

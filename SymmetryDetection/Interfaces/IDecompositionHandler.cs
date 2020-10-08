using System;
namespace SymmetryDetection.Interfaces
{
    public interface IDecompositionHandler
    {
        bool Decompose(ref double[,] original);

        double[,] GetQR();
        double[] GetGammas();
        double[,] GetR();
        double[,] GetQ();
    }
}

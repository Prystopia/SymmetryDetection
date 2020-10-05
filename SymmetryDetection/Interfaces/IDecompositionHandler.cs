using System;
namespace SymmetryDetection.Interfaces
{
    public interface IDecompositionHandler
    {
        bool Decompose(float[,] original);

        float[][] GetQR();
        float[] GetGammas();
        float[,] GetR(float[,] R, bool compact);
    }
}

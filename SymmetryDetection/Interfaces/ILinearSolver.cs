using System;
namespace SymmetryDetection.Interfaces
{
    public interface ILinearSolver
    {

        bool SetA(float[,] A);
        float[,] Solve(float[,] B);
    }
}

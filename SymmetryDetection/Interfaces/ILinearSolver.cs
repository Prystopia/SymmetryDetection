using System;
namespace SymmetryDetection.Interfaces
{
    public interface ILinearSolver
    {

        bool SetA(double[,] A);
        double[,] Solve(double[,] B);
    }
}

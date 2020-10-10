using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Interfaces
{
    public interface ILMFunction
    {
        double[] Function(double[] input);
        //int df(float[] input, float[,] fjac);
        int InputSize { get; }
        int ValuesSize { get; }

    }
}

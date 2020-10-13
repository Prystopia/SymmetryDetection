using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Interfaces
{
    public interface IOptimisationFunction
    {
        double[] Function(double[] input);
        int InputSize { get; }
        int ValuesSize { get; }
    }
}

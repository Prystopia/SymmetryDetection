using System;
using System.Numerics;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetry
    {
        Vector3 Origin { get; set; }
        Vector3 Normal { get; set; }

        void SetOriginProjected(Vector3 point);
    }
}

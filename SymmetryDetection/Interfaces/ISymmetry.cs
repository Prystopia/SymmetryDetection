using SymmetryDetection.Enums;
using System;
using System.Numerics;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetry
    {
        Vector3 Origin { get; set; }
        Vector3 Normal { get; set; }
        float SymmetryScore { get; set; }
        SymmetryTypeEnum SymmetryType { get; set; }

        void SetOriginProjected(Vector3 point);
    }
}

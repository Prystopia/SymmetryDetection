using SymmetryDetection.Extensions;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class ReflectionalSymmetry : ISymmetry
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }
        public ReflectionalSymmetry(Vector3 origin, Vector3 normal)
        {
            this.Origin = origin;
            this.Normal = Vector3.Normalize(normal);
        }

        public void SetOriginProjected(Vector3 point)
        {
            Origin = PointHelpers.ProjectPoint(point, this);
        }
    }
}

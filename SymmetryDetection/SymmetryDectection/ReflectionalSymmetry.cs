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
        public override string ToString()
        {
            var d = ((Normal.X * Origin.X) + (Normal.Y * Origin.Y) + (Normal.Z * Origin.Z));
            return $"({MathF.Round(d,2)} - {MathF.Round(Normal.X, 2)} * (x - {Math.Round(Origin.X, 2)}) - {MathF.Round(Normal.Y, 2)} * (y - {MathF.Round(Origin.Y, 2)}) - {MathF.Round(Origin.Z * Normal.Z, 2)}) / {MathF.Round(Normal.Z, 2)})";
        }
    }
}

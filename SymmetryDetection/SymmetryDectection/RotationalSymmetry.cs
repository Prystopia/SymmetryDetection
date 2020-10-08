using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class RotationalSymmetry : ISymmetry
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }

        public RotationalSymmetry(Vector3 origin, Vector3 rotation)
        {
            this.Origin = origin;
            this.Normal = rotation;
        }

        public void SetOriginProjected(Vector3 point)
        {
            throw new NotImplementedException();
        }
    }
}

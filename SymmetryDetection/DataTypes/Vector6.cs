using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.DataTypes
{
    public class Vector6
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        //not sure what these should be termed but leave for now
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }

        public Vector3 Head => new Vector3(X, Y, Z);
        public Vector3 Tail => new Vector3(A, B, C);
    }
}

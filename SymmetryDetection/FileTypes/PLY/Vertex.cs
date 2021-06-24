using System;
using System.Collections.Generic;
using System.Numerics;

namespace SymmetryDetection.FileTypes.PLY
{
    public class Vertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public float Curvature { get; set; }
        public Dictionary<string, object> AdditionalProperties { get; set; }
    }
}

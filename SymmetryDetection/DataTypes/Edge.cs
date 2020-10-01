using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.DataTypes
{
    public class Edge
    {
        public int NodeFrom { get; set; }
        public int NodeTo { get; set; }
        public Edge(int from, int to)
        {
            NodeFrom = from;
            NodeTo = to;
        }
    }
}

using SymmetryDetection.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class Correspondence
    {
        public PointXYZNormal Original { get; set; }
        public PointXYZNormal CorrespondingPoint { get; set; }
        public float Distance { get; set; }

        public Correspondence(PointXYZNormal original, PointXYZNormal neighbour, float distance)
        {
            this.Original = original;
            this.CorrespondingPoint = neighbour;
            this.Distance = distance;
        }
    }
}

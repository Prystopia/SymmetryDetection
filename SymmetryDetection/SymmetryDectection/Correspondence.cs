using SymmetryDetection.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class Correspondence
    {
        public PointXYZRGBNormal Original { get; set; }
        public PointXYZRGBNormal CorrespondingPoint { get; set; }
        public float Distance { get; set; }

        public Correspondence(PointXYZRGBNormal original, PointXYZRGBNormal neighbour, float distance)
        {
            this.Original = original;
            this.CorrespondingPoint = neighbour;
            this.Distance = distance;
        }
    }
}

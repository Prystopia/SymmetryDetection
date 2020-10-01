using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.DataTypes
{
    public class PointXYZRGBNormal : ICloneable<PointXYZRGBNormal>
    {
        public Guid Id { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Color Colour { get; set; }
        public float Curvature { get; set; }

        public PointXYZRGBNormal()
        {
            Id = Guid.NewGuid();
        }

        public float GetDistance(Vector3 point)
        {
            var xDist = Math.Pow(point.X - Position.X, 2);
            var yDist = Math.Pow(point.Y - Position.Y, 2);
            var zDist = Math.Pow(point.Z - Position.Z, 2);
            return (float)Math.Sqrt(xDist + yDist + zDist);
        }
        public PointXYZRGBNormal Clone()
        {
            PointXYZRGBNormal newPoint = new PointXYZRGBNormal();
            newPoint.Colour = Color.FromArgb(this.Colour.ToArgb());
            newPoint.Position = new Vector3(Position.X, Position.Y, Position.Z);
            newPoint.Normal = new Vector3(Normal.X, Normal.Y, Normal.Z);
            newPoint.Curvature = Curvature;
            newPoint.Id = Id;
            return newPoint;
        }

        
    }
}

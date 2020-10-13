using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.DataTypes
{
    public class PointXYZNormal : ICloneable<PointXYZNormal>
    {
        public Guid Id { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }

        public PointXYZNormal()
        {
            Id = Guid.NewGuid();
        }

        public float GetDistance(Vector3 point)
        {
            var xDist = MathF.Pow(Position.X - point.X, 2);
            var yDist = MathF.Pow(Position.Y - point.Y, 2);
            var zDist = MathF.Pow(Position.Z - point.Z, 2);
            return xDist + yDist + zDist;
        }
        public PointXYZNormal Clone()
        {
            PointXYZNormal newPoint = new PointXYZNormal();
            newPoint.Position = new Vector3(Position.X, Position.Y, Position.Z);
            newPoint.Normal = new Vector3(Normal.X, Normal.Y, Normal.Z);
            newPoint.Id = Id;
            return newPoint;
        }

        
    }
}

using SymmetryDetection.Extensions;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.DataTypes
{
    public class PointCloud: ICloneable<PointCloud>
    {
        public int Width => Points.Count;
        public int Height { get; set; }
        public List<PointXYZNormal> Points { get; set; }
        public bool IsDense { get; set; }

        public PointCloud()
        {
            Points = new List<PointXYZNormal>();
            //this indicates that we have no NaN / infinite values in our data which should not be a problem for this
            IsDense = true;
            Height = 1;
        }
        
        public void AddPoint(PointXYZNormal point)
        {
            this.Points.Add(point);

        }

        public PointCloud Clone()
        {
            PointCloud newCloud = new PointCloud();
            newCloud.Height = Height;
            newCloud.IsDense = IsDense;
            foreach(var point in Points)
            {
                newCloud.AddPoint(point.Clone());
            }

            return newCloud;
        }
        public class NeigbourInfo
        {
            public PointXYZNormal neighbour;
            public float distance;
            public int index;
        }
        public class PointInfo
        {
            public int index;
            public PointXYZNormal point;
        }

        public List<NeigbourInfo> GetNeighbours(PointXYZNormal initial, float? searchRadius = 1)
        {
            List<NeigbourInfo> neighbourPoints = new List<NeigbourInfo>();

            IEnumerable<PointInfo> reducedPoints = Points.Select((p, i) => new PointInfo() { index = i, point = p });
            if (searchRadius.HasValue)
            {
                //reduce number of points - only need to check for search radius in x, y & z to find items
                Vector3 initialPos = initial.Position;
                var max = new Vector3(initialPos.X + searchRadius.Value, initialPos.Y + searchRadius.Value, initialPos.Z + searchRadius.Value);
                var min = new Vector3(initialPos.X - searchRadius.Value, initialPos.Y - searchRadius.Value, initialPos.Z - searchRadius.Value);
                reducedPoints = Points.Select((p, i) => new PointInfo() { point = p, index = i }).Where(p => p.point.Position.X >= min.X && p.point.Position.X <= max.X &&
                                                       p.point.Position.Y >= min.Y && p.point.Position.Y <= max.Y &&
                                                       p.point.Position.Z >= min.Z && p.point.Position.Z <= max.Z);
            }
            foreach (var point2 in reducedPoints)
            {
                if (initial.Id != point2.point.Id)
                {
                    var distance = point2.point.GetDistanceSquared(initial.Position);
                    if (!searchRadius.HasValue || distance <= searchRadius)
                    {
                        neighbourPoints.Add(new NeigbourInfo() { neighbour = point2.point, distance = distance, index = point2.index });
                    }
                }
            }
            return neighbourPoints;
        }

        public List<NeigbourInfo> GetClosetNeighbours(PointXYZNormal initial, int neighboursToReturn)
        {
            var neighbours = GetNeighbours(initial, null);
            var orderedNeighbours = neighbours.OrderBy(n => n.distance);
            return orderedNeighbours.Take(neighboursToReturn).ToList();
        }
    }
}

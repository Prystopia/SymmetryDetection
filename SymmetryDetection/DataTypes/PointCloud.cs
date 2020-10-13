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

        public List<(PointXYZNormal neighbour, float distanceSquared, int index)> GetNeighbours(PointXYZNormal initial, float? searchRadius = 1)
        {
            int count = 0;
            List<(PointXYZNormal neighbour, float distanceSquared, int index)> neighbourPoints = new List<(PointXYZNormal neighbour, float distanceSquared, int index)>();
            foreach (var point2 in Points)
            {
                if (initial.Id != point2.Id)
                {
                    var distance = point2.GetDistance(initial.Position);
                    if (!searchRadius.HasValue || distance <= searchRadius)
                    {
                        neighbourPoints.Add((point2, distance, count));
                    }
                }
                count++;
            }
            return neighbourPoints;
        }

        public List<(PointXYZNormal neighbour, float distance, int index)> GetClosetNeighbours(PointXYZNormal initial, int neighboursToReturn)
        {
            var neighbours = GetNeighbours(initial, null);
            var orderedNeighbours = neighbours.OrderBy(n => n.distanceSquared);
            return orderedNeighbours.Take(neighboursToReturn).ToList();
        }
    }
}

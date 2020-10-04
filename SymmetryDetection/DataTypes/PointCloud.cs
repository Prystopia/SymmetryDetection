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
        public List<PointXYZRGBNormal> Points { get; set; }
        public bool IsDense { get; set; }
        public Vector3 Mean { get; set; }

        public PointCloud()
        {
            Points = new List<PointXYZRGBNormal>();
            //this indicates that we have no NaN / infinite values in our data which should not be a problem for this
            IsDense = true;
            Height = 1;

        }
        
        public void AddPoint(PointXYZRGBNormal point)
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

        public List<(PointXYZRGBNormal neighbour, float distanceSquared)> GetNeighbours(PointXYZRGBNormal initial, float? searchRadius = 1)
        {
            List<(PointXYZRGBNormal neighbour, float distanceSquared)> neighbourPoints = new List<(PointXYZRGBNormal neighbour, float distanceSquared)>();
            foreach (var point2 in Points)
            {
                var distance = point2.GetDistance(initial.Position);
                if (!searchRadius.HasValue || distance <= searchRadius)
                {
                    neighbourPoints.Add((point2, distance));
                }
               
            }
            return neighbourPoints;
        }

        public List<(PointXYZRGBNormal neighbour, float distance)> GetClosetNeighbours(PointXYZRGBNormal initial, int neighboursToReturn)
        {
            var neighbours = GetNeighbours(initial, null);
            var orderedNeighbours = neighbours.OrderBy(n => n.distanceSquared);
            return orderedNeighbours.Take(neighboursToReturn).ToList();
        }

        public void GetCloudBoundary()
        {
            List<PointXYZRGBNormal> boundaryPointIds = new List<PointXYZRGBNormal>();
            List<PointXYZRGBNormal> nonBoundaryPointIds = new List<PointXYZRGBNormal>();

            
            foreach(var point in Points)
            {
                //get all other points within the specified radius
                var neighbourPoints = GetNeighbours(point);
                if (IsBoundaryPoint(point, neighbourPoints))
                {
                    boundaryPointIds.Add(point);
                }
                else
                {
                    nonBoundaryPointIds.Add(point);
                }
            }

        }
        private bool IsBoundaryPoint(PointXYZRGBNormal point, List<(PointXYZRGBNormal neighbour, float distanceSquared)> neighbours)
        {
            bool isBoundary = false;
            List<Vector3> projectedNeighbours = new List<Vector3>();

            if(neighbours.Any())
            {
                var planePoint = point.Position;
                var planeNormal = point.Normal;
                foreach(var neighbour in neighbours)
                {
                    var neighbourPos = neighbour.neighbour.Position;
                    var projectedPoint = ProjectToPlane(neighbourPos, planePoint, planeNormal);
                    projectedNeighbours.Add(projectedPoint);
                }

                Vector3 referenceVector = projectedNeighbours[0] - planePoint;
                List<float> angles = new List<float>();
                foreach(var projected in projectedNeighbours)
                {
                    Vector3 currentVector = projected - planePoint;
                    float curAngle = vectorVectorAngleCW(referenceVector, currentVector, planeNormal);
                    angles.Add(curAngle);
                }
                angles.Sort();
                List<float> angleDifferences = new List<float>();
                for(int i = 1; i < angles.Count; i++)
                {
                    angleDifferences.Add(AngleDifferentCCW(angles[i - 1], angles[i]));
                }
                angleDifferences[0] = AngleDifferentCCW(angles.Last(), angles[0]);

                if(angleDifferences.Max() > (135f).ConvertToRadians())
                {
                    isBoundary = true;
                }
            }
            else
            {
                isBoundary = true;
            }

            return isBoundary;
        }

        public void GetMinMax(out Vector3 min, out Vector3 max)
        {
            min = new Vector3(float.MaxValue);
            max = new Vector3(float.MinValue);
            foreach (var point in Points)
            {
                min = Vector3.Min(min, point.Position);
                max = Vector3.Max(max, point.Position);
            }
        }

        private Vector3 ProjectToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return point - (planeNormal * PointToPlaneSignedDistance(point, planePoint, planeNormal));
        }

        private float PointToPlaneSignedDistance(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            var planeToPointVector = point - planePoint;
            return Vector3.Dot(planeToPointVector, planeNormal);
        }

        private float vectorVectorAngleCW(Vector3 reference, Vector3 current, Vector3 planeNormal)
        {
            float cos = Vector3.Dot(reference, current);
            float sin = Vector3.Dot(planeNormal, Vector3.Cross(reference, current)).ClampValue(-1, 1);
            return (float)Math.Atan2(sin, cos);
        }
        private float AngleDifferentCCW(float prevAngle, float curAngle)
        {
            float mod = (curAngle - prevAngle) % (2 * MathF.PI);
            if(mod < 0)
            {
                mod += MathF.Abs(2 * MathF.PI);
            }
            return mod;
        }

        private void Calculate3DCentroid()
        {
            if (IsDense)
            {
                float x = 0, y = 0, z = 0;
                foreach (var point in Points)
                {
                    x += point.Position.X;
                    y += point.Position.Y;
                    z += point.Position.Z;
                }
                Mean = new Vector3(x, y, z);
                Mean /= Points.Count;
            }
        }

    }
}

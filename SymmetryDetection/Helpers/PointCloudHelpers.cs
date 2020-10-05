using System;
using System.Numerics;
using SymmetryDetection.DataTypes;

namespace SymmetryDetection.Helpers
{
    public class PointCloudHelpers
    {
        public static PointCloud ProjectCloudToPlane(PointCloud original, Vector3 planePoint, Vector3 planeNormal)
        {
            PointCloud projected = original.Clone();
            foreach (var point in projected.Points)
            {
                point.Position = PointHelpers.ProjectPointToPlane(point.Position, planePoint, planeNormal);
            }
            return projected;
        }

        
    }
}

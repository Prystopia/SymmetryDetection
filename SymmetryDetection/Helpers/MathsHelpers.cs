using System;
using System.Numerics;

namespace SymmetryDetection.Helpers
{
    public class MathsHelpers
    {
        public static float PointToPointDistance(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        public static float PointToPlaneSignedDistance(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            Vector3 planeToPointVector = point - planePoint;
            return Vector3.Dot(planeToPointVector, planeNormal);
        }
    }
}

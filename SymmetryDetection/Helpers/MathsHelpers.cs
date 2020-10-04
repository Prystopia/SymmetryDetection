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

        public static float[,] CalculateAngleAxis(float angle, Vector3 axis)
        {
            float[,] res = new float[3, 3];
            Vector3 sin_axis = MathF.Sin(angle) * axis;
            float c = MathF.Cos(angle);
            Vector3 cos1_axis = (1 - c) * axis;

            float tmp;
            tmp = cos1_axis.X * axis.Y;
            res[0, 1] = tmp - sin_axis.Z;
            res[1, 0] = tmp + sin_axis.Z;

            tmp = cos1_axis.X * axis.Z;
            res[0, 2] = tmp + sin_axis.Y;
            res[2, 0] = tmp - sin_axis.Y;

            tmp = cos1_axis.Y * axis.Z;
            res[1, 2] = tmp - sin_axis.X;
            res[2, 1] = tmp + sin_axis.X;
            //res.diagonal() = Vector3.Cross(cos1_axis, axis) + c;
            return res;
        }
    }
}

using System;
using System.Numerics;
using SymmetryDetection.Extensions;
using SymmetryDetection.Interfaces;

namespace SymmetryDetection.Helpers
{
    public class PointHelpers
    {
        public static Vector3 ProjectPointToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return point - planeNormal * MathsHelpers.PointToPlaneSignedDistance(point, planePoint, planeNormal);
        }

        public static Vector3 ReflectNormal(Vector3 normal, ISymmetry plane)
        {
            return (normal - 2 * (Vector3.Dot(normal, plane.Normal) * plane.Normal));
        }

        public static Vector3 ReflectPoint(Vector3 point, ISymmetry plane)
        {
            return (point - 2 * plane.Normal * (Vector3.Dot(plane.Normal, point - plane.Origin)));
        }
        public static float PointSignedDistance(Vector3 point, ISymmetry plane)
        {
            Vector3 planeToPointVector = point - plane.Origin;
            return Vector3.Dot(planeToPointVector, plane.Normal);
        }

        public static void ReflectedSymmetryDifference(ISymmetry other, Vector3 referencePoint, ISymmetry symmetry, out float angle, out float distance)
        {
            angle = (float)Math.Acos(LineLineAngleCos(symmetry.Normal, other.Normal));
            Vector3 refProjectPoint1 = ProjectPoint(referencePoint, symmetry);
            Vector3 refProjectPoint2 = ProjectPoint(referencePoint, other);
            distance = MathsHelpers.PointToPointDistance(refProjectPoint1, refProjectPoint2);
        }

        private static float LineLineAngleCos(Vector3 direction1, Vector3 direction2)
        {
            return Math.Abs(VectorVectorAngleCos(direction1, direction2));
        }
        private static float VectorVectorAngleCos(Vector3 direction1, Vector3 direction2)
        {
            return Vector3.Dot(direction1, direction2).ClampValue(-1, 1);
        }


        public static Vector3 ProjectPoint(Vector3 point, ISymmetry plane)
        {
            return ProjectPointToPlane(point, plane.Origin, plane.Normal);
        }
    }
}

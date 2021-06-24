using System;
using System.Numerics;
using SymmetryDetection.Extensions;
using SymmetryDetection.Interfaces;

namespace SymmetryDetection.Helpers
{
    public static class ReflectionHelpers
    {
        //public static float GetReflectionSymmetryNormalFitError(Vector3 normal1, Vector3 normal2, ISymmetry symmetry)
        //{
        //    Vector3 normal2Reflected = PointHelpers.ReflectNormal(normal2, symmetry);

        //    float dotProd = Vector3.Dot(normal1, normal2Reflected).ClampValue(-1, 1);

        //    return MathF.Acos(dotProd);
        //}

        public static float GetReflectionSymmetryPositionFitError(Vector3 point1, Vector3 point2, ISymmetry symmetry)
        {
            //get distance of midpoint between two points to the plane
            Vector3 midpoint = (point1 + point2) / 2;
            //get distance from midpoint to the plane of symmetry
            return PointHelpers.PointSignedDistance(midpoint, symmetry);
        }
    }
}

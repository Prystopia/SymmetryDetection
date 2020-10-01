using SymmetryDetection.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class ReflectionalSymmetry
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }
        public ReflectionalSymmetry(Vector3 origin, Vector3 normal)
        {
            this.Origin = origin;
            this.Normal = Vector3.Normalize(normal);
        }

        public Vector3 ReflectNormal(Vector3 normal)
        {
            return (normal - 2 * (Vector3.Dot(normal, Normal) * Normal));
        }

        public Vector3 ReflectPoint(Vector3 point)
        {
            return (point - 2 * Normal * (Vector3.Dot(Normal, point - Origin)));
        }
        public float PointSignedDistance(Vector3 point)
        {
            Vector3 planeToPointVector = point - Origin;
            return Vector3.Dot(planeToPointVector, Normal);
        }

        public void SetOriginProjected(Vector3 point)
        {
            Origin = ProjectPoint(point);
        }

        public void ReflectedSymmetryDifference(ReflectionalSymmetry other, Vector3 referencePoint, out float angle, out float distance)
        {
            angle = MathF.Acos(LineLineAngleCos(Normal, other.Normal));
            Vector3 refProjectPoint1 = ProjectPoint(referencePoint);
            Vector3 refProjectPoint2 = other.ProjectPoint(referencePoint);
            distance = ReflectionalSymmetryDetection.PointToPointDistance(refProjectPoint1, refProjectPoint2);
        }

        private float LineLineAngleCos(Vector3 direction1, Vector3 direction2)
        {
            return MathF.Abs(VectorVectorAngleCos(direction1, direction2));
        }
        private float VectorVectorAngleCos(Vector3 direction1, Vector3 direction2)
        {
            return Vector3.Dot(direction1, direction2).ClampValue(-1, 1);
        }


        private Vector3 ProjectPoint(Vector3 point)
        {
            return ReflectionalSymmetryDetection.ProjectPointToPlane(point, Origin, Normal);
        }

    }
}

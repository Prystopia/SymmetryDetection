using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class RotationalSymmetry : ISymmetry
    {
        public Vector3 Origin { get; set; }
        public Vector3 Normal { get; set; }
        public float PerpendicularScore { get; set; }
        public float CoverageScore { get; set; }
        public float SymmetryScore { get; set; }
        public float OcclusionScore { get; set; }

        public RotationalSymmetry(Vector3 origin, Vector3 rotation)
        {
            this.Origin = origin;
            this.Normal = rotation;
        }

        public void SetOriginProjected(Vector3 point)
        {
            Origin = PointHelpers.ProjectPoint(point, this);
        }

        public float GetRotationalFitError(Vector3 position, Vector3 normal)
        {
            Vector3 projectedPoint = PointHelpers.ProjectPoint(position, this);
            Vector3 planeNormal = Vector3.Cross((position - projectedPoint), Normal);

            float angleSin = MathF.Abs(Vector3.Dot(planeNormal, normal) / planeNormal.Norm()).ClampValue(0, 1);
            return MathF.Asin(angleSin);
        }

        public float GetRotationalSymmetryPerpendicularity(Vector3 normal, float angleThreshold)
        {
            var normalAngle = MathF.Acos(PointHelpers.LineLineAngleCos(Normal, normal));
            normalAngle /= angleThreshold;
            normalAngle = MathF.Min(normalAngle, 1.0f);

            return 1.0f - normalAngle;
        }
    }
}

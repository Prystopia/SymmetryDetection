using SymmetryDetection.DataTypes;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryScorers
{
    public class CloudCoverageScoreService : ISymmetryScoreService
    {
        public List<float> CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistances, out List<Correspondence> correspondences)
        {
            float score = 0;
            correspondences = new List<Correspondence>();

            // Get reference vector
            Vector3 referenceVector = cloud.Points[0].Position - PointHelpers.ProjectPoint(cloud.Points[0].Position, symmetry);

            // Find angles between vectors formed by all other points and current vector.
            List<float> angles = new List<float>();
            angles[0] = 0.0f;
            foreach(var point in cloud.Points)
            {
                var reflectedPointPosition = PointHelpers.ProjectPoint(point.Position, symmetry);
                Vector3 currentVector = point.Position - reflectedPointPosition;
                angles.Add(MathsHelpers.VectorVectorAngleCW(referenceVector, currentVector, symmetry.Normal));
            }

            //sort the angles
            angles = angles.OrderBy(a => a).ToList();

            // Get angle difference
            List<float> angleDifferences = new List<float>();
            angleDifferences.Add(MathsHelpers.AngleDifferenceCCW(angles[angles.Count - 1], angles[0]));

            for(int i = 1; i < angles.Count; i++)
            {
                float angle = angles[i];
                angleDifferences.Add(MathsHelpers.AngleDifferenceCCW(angles[i - 1], angle));
            }

            score = (2.0f * MathF.PI) - angleDifferences.Max();

            return new List<float>() { score };
        }
    }
}

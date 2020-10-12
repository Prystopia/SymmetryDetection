using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.Parameters;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryScorers
{
    public class ReflectionalSymmetryScoreService : ISymmetryScoreService
    {
        public List<float> CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistances, out List<Correspondence> correspondences)
        {
            List<float> pointSymmetryScores = new List<float>();
            correspondences = new List<Correspondence>();

            float minInlierNormalAngle = (10f).ConvertToRadians();
            float maxInlierNormalAngle = (15f).ConvertToRadians();

            for (int i = 0; i < cloud.Points.Count; i++)
            {
                var point = cloud.Points[i];
                // Get point normal
                Vector3 srcPoint = point.Position;
                Vector3 srcNormal = point.Normal;

                // Reflect point and normal
                Vector3 srcPointReflected = PointHelpers.ReflectPoint(srcPoint, symmetry);
                Vector3 srcNormalReflected = PointHelpers.ReflectNormal(srcNormal, symmetry);

                // Find neighbours in a radius   
                PointXYZRGBNormal searchPoint = new PointXYZRGBNormal()
                {
                    Position = srcPointReflected,
                    Normal = srcNormalReflected,
                };

                var neighbours = cloud.GetClosetNeighbours(searchPoint, 1);
                if (ignoreDistances || neighbours[0].distance <= ReflectionalSymmetryParameters.MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE)
                {
                    Vector3 targetPoint = neighbours[0].neighbour.Position;

                    float symmetryScore = ReflectionHelpers.GetReflectionSymmetryPositionFitError(srcPoint, targetPoint, symmetry);

                    symmetryScore = (symmetryScore - minInlierNormalAngle) / (maxInlierNormalAngle - minInlierNormalAngle);
                    symmetryScore = symmetryScore.ClampValue(0f, 1f);

                    // If all checks passed - add correspondence
                    pointSymmetryScores.Add(symmetryScore);
                    correspondences.Add(new Correspondence(point, neighbours[0].neighbour, neighbours[0].distance));
                }
            }

            return pointSymmetryScores;
        }
    }
}

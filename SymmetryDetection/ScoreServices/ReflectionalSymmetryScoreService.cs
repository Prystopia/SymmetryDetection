using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.ScoreServices
{
    public class ReflectionalSymmetryScoreService : IScoreService
    {
        private const float MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE = 0.5f;
        public void CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistance, out List<float> pointSymmetryScores, out List<Correspondence> correspondences)
        {
            pointSymmetryScores = new List<float>();
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
                if (ignoreDistance || neighbours[0].distance <= MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE)
                {
                    Vector3 targetPoint = neighbours[0].neighbour.Position;
                    Vector3 targetNormal = neighbours[0].neighbour.Normal;

                    // If point belongs to segment boundary, we reduce it's score in half, since normals at the boundary of the segment are usually noisy - noise isn't something we need to worry about so ignore for the moment
                    //if (std::find(cloud_ds_boundary_point_ids.begin(), cloud_ds_boundary_point_ids.end(), pointId) != cloud_ds_boundary_point_ids.end() ||
                    //      std::find(cloud_boundary_point_ids.begin(), cloud_boundary_point_ids.end(), neighbours[0]) != cloud_boundary_point_ids.end())
                    //    continue;

                    float symmetryScore = ReflectionHelpers.GetReflectionSymmetryPositionFitError(srcPoint, targetPoint, symmetry);

                    // NOTE: this is to avoid the problem with correspondences between thin walls.
                    // Correspondences between thin walls are likely to have normals that are 180 degrees appart.
                    if (symmetryScore > MathF.PI * 3 / 4)
                        symmetryScore = MathF.PI - symmetryScore;

                    symmetryScore = (symmetryScore - minInlierNormalAngle) / (maxInlierNormalAngle - minInlierNormalAngle);
                    symmetryScore = symmetryScore.ClampValue(0f, 1f);

                    // If all checks passed - add correspondence
                    pointSymmetryScores.Add(symmetryScore);
                    correspondences.Add(new Correspondence(point, neighbours[0].neighbour, neighbours[0].distance));
                }
            }
        }
    }
}

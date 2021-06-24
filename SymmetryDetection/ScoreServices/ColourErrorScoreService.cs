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

namespace SymmetryDetection.ScoreServices
{
    public class ColourErrorScoreService : IScoreService<ReflectionalSymmetryParameters>
    {
        public void CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistance, ReflectionalSymmetryParameters parameters,  out List<float> pointSymmetryScores, out List<Correspondence> correspondences)
        {
            pointSymmetryScores = new List<float>();
            correspondences = new List<Correspondence>();

            for (int i = 0; i < cloud.Points.Count; i++)
            {
                var point = cloud.Points[i];
                // Get point normal
                Vector3 srcPoint = point.Position;
                Vector3 srcNormal = point.Normal;
                float srcPointColourIntensity = (point.Colour.X + point.Colour.Y + point.Colour.Z) / 3;

                // Reflect point and normal
                Vector3 srcPointReflected = PointHelpers.ReflectPoint(srcPoint, symmetry);
                Vector3 srcNormalReflected = PointHelpers.ReflectNormal(srcNormal, symmetry);

                // Find neighbours in a radius   
                PointXYZNormal searchPoint = new PointXYZNormal()
                {
                    Position = srcPointReflected,
                    Normal = srcNormalReflected,
                    Id = point.Id,
                    Colour = point.Colour,
                };

                var neighbours = cloud.GetClosetNeighbours(searchPoint, 1);
                if (ignoreDistance || neighbours[0].distance <= parameters.MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE)
                {
                    var neighbourColour = neighbours[0].neighbour.Colour;
                    float targetPointColourIntensity = (neighbourColour.X + neighbourColour.Y + neighbourColour.Z) / 3;

                    float colourError = Math.Abs(srcPointColourIntensity - targetPointColourIntensity) / 255;

                    //float colourScore = 1 / (1 + colourError);

                    // If all checks passed - add correspondence
                    pointSymmetryScores.Add(colourError);
                    correspondences.Add(new Correspondence(point, neighbours[0].neighbour, neighbours[0].distance));
                }
            }
        }
    }
}

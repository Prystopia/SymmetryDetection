using SymmetryDetection.DataTypes;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.SymmetryScorers
{
    public class PerpendicularScoreService : ISymmetryScoreService
    {
        public List<float> CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistances, out List<Correspondence> correspondences)
        {
            correspondences = new List<Correspondence>();
            float scoreSum = 0;
            float score = 0;
            float angleThreshold = MathF.PI / 2;

            foreach (var point in cloud.Points)
            {
                scoreSum += ((RotationalSymmetry)symmetry).GetRotationalSymmetryPerpendicularity(point.Normal, angleThreshold);
            }
            score = scoreSum / cloud.Points.Count;

            return new List<float>() { score };
        }
    }
}

using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.SymmetryScorers
{
    public class RotationalSymmetryScoreService : ISymmetryScoreService
    {
        public List<float> CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistances, out List<Correspondence> correspondences)
        {
            correspondences = new List<Correspondence>();
            float score = 0;
            float scoreSum = 0;

            foreach(var point in cloud.Points)
            {
                var position = point.Position;
                var normal = point.Normal;
                float angle = ((RotationalSymmetry)symmetry).GetRotationalFitError(position, normal);
                scoreSum += (angle / (MathF.PI / 2)).ClampValue(0, 1);
            }

            score = scoreSum / cloud.Points.Count;

            return new List<float>() { score };
        }
    }
}

using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.ScoreServices
{
    public class AverageGlobalScoreService : IGlobalScoreService
    {
        public float CalculateGlobalScore(SymmetryDetectionHandler handler)
        {
            float scoreSum = 0;

            foreach(var symmetry in handler.Symmetries)
            {
                scoreSum += symmetry.SymmetryScore;
            }
            scoreSum /= (handler.Symmetries.Count > 0 ? handler.Symmetries.Count : 1);
            scoreSum *= ((float)handler.Symmetries.Count / 13);

            return scoreSum;
        }
    }
}

using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.ScoreServices
{
    public class BestPlaneGlobalScoreService : IGlobalScoreService
    {
        public float CalculateGlobalScore(SymmetryDetectionHandler handler)
        {
            float bestScore = float.MinValue;

            foreach (var symmetry in handler.Symmetries)
            {
                if(symmetry.SymmetryScore > bestScore)
                {
                    bestScore = symmetry.SymmetryScore;
                }
            }

            if(bestScore == float.MinValue)
            {
                bestScore = 0;
            }

            return bestScore;
        }

        public float CalculateGlobalScore(ISymmetryDetector handler)
        {
            float bestScore = float.MinValue;

            foreach (var symmetry in handler.MergedSymmetries)
            {
                if (symmetry.SymmetryScore > bestScore)
                {
                    bestScore = symmetry.SymmetryScore;
                }
            }

            if (bestScore == float.MinValue)
            {
                bestScore = 0;
            }

            return bestScore;
        }
    }
}

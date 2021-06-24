using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Interfaces
{
    public interface IGlobalScoreService
    {
        float CalculateGlobalScore(SymmetryDetectionHandler handler);
    }
}

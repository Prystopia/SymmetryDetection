using SymmetryDetection.DataTypes;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetryScoreService
    {
        List<float> CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistances, out List<Correspondence> correspondences);
    }
}

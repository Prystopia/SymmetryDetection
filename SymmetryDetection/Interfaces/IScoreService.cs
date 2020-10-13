using SymmetryDetection.DataTypes;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Interfaces
{
    public interface IScoreService<T> where T : ISymmetryParameters
    {
        void CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, bool ignoreDistances, T parameters, out List<float> pointSymmetryScores, out List<Correspondence> correspondences);
    }
}

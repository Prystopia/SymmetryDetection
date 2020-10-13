using System;
using System.Collections.Generic;
using System.Numerics;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Enums;
using SymmetryDetection.SymmetryDectection;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetryDetector
    {
        SymmetryTypeEnum SymmetryType { get; }
        List<ISymmetry> MergedSymmetries { get; set; }
        void SetCloud(PointCloud cloud);
        void SetPCA(PCA pca);
        void Detect();
        void Filter();
        void Merge();
        float CalculateGlobalScore();
    }
}

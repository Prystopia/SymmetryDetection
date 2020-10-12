using System;
using System.Collections.Generic;
using System.Numerics;
using SymmetryDetection.DataTypes;
using SymmetryDetection.SymmetryDectection;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetryDetector<T> where T : ISymmetry
    {
        int MaxPlanes { get; }
        List<T> RefinedSymmetries { get; set; }
        List<T> FilteredSymmetries { get; set; }
        List<T> MergedSymmetries { get; set; }
        void SetCloud(PointCloud cloud);
        void SetPCA(PCA pca);
        void Detect();
        void Filter();
        void Merge();
        List<T> MergeDuplicateSymmetries(List<T> symmetries, List<Vector3> symmetryReferencePoints);
        List<T> GetInitialSymmetries(PointCloud cloud);
        float CalculateGlobalSymmetryScore();
    }
}

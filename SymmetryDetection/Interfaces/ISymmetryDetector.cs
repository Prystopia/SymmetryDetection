using System;
using System.Collections.Generic;
using System.Numerics;
using SymmetryDetection.DataTypes;
using SymmetryDetection.SymmetryDectection;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetryDetector
    {
        int MaxPlanes { get; }
        List<ISymmetry> RefinedSymmetries { get; set; }
        List<float> OcclusionScores { get; set; }
        List<float> CloudInlierScores { get; set; }
        List<float> CorrespondenceInlierScores { get; set; }
        List<ISymmetry> FilteredSymmetries { get; set; }
        List<ISymmetry> MergedSymmetries { get; set; }
        void SetCloud(PointCloud cloud);
        void SetPCA(PCA pca);
        void Detect();
        void Filter();
        void Merge();
        List<ISymmetry> MergeDuplicateSymmetries(List<ISymmetry> symmetries, List<Vector3> symmetryReferencePoints);
        List<ISymmetry> GetInitialSymmetries(PointCloud cloud);
        float CalculateGlobalSymmetryScore();
    }
}

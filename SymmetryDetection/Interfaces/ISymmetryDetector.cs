using System;
using System.Collections.Generic;
using System.Numerics;
using SymmetryDetection.DataTypes;
using SymmetryDetection.SymmetryDectection;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetryDetector
    {
        List<ISymmetry> SymmetriesRefined { get; set; }
        List<float> OcclusionScores { get; set; }
        List<float> CloudInlierScores { get; set; }
        List<float> CorrespondenceInlierScores { get; set; }
        List<int> SymmetryFilteredIds { get; set; }
        List<int> SymmetryMergedIds { get; set; }
        void SetCloud(PointCloud cloud);
        void SetPCA(PCA pca);
        void Detect();
        void Filter();
        void Merge();
        List<int> MergeDuplicateSymmetries(List<ISymmetry> symmetries, List<int> indices, List<Vector3> symmetryReferencePoints, List<float> occlusionScores);
        List<ISymmetry> GetInitialSymmetries(PointCloud cloud);
        ISymmetry RefineSymmetryPosition(PointCloud cloud, ISymmetry originalSymmetry);
        bool RefineGlobalSymmetryPosition(PointCloud cloud, ISymmetry symmetry);
        float CalculateGlobalScore();
    }
}

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
        List<int> MergeDuplicateReflectedSymmetries(List<ISymmetry> symmetries, List<Vector3> symmetryReferencePoints, List<float> occlusionScores);
        List<ISymmetry> GetInitialReflectionSymmetries(PointCloud cloud);
        List<Vector3> GenerateSpherePoints(int numSegments);
        bool RefineSymmetryPosition(PointCloud cloud, ISymmetry originalSymmetry);
        bool RefineGlobalSymmetryPosition(PointCloud cloud, ISymmetry symmetry);
        void CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, out List<float> pointSymmetryScores, out List<Correspondence> correspondences);
    }
}

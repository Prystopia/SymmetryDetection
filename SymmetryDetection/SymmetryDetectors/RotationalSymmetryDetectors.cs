using SymmetryDetection.DataTypes;
using SymmetryDetection.Enums;
using SymmetryDetection.Interfaces;
using SymmetryDetection.Optimisation;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDetectors
{
    public class RotationalSymmetryDetectors : ISymmetryDetector
    {
        public SymmetryTypeEnum SymmetryType => SymmetryTypeEnum.Rotational;
        public const float MAX_SYMMETRY_SCORE = 0;
        private const float MAX_OCCLUSION_SCORE = 0;
        private const float MAX_PERPENDICULAR_SCORE = 0;
        private const float MIN_COVERAGE_SCORE = 0;
        private const float REFERENCE_MAX_FIT_ANGLE = 0;

        public List<ISymmetry> SymmetriesRefined { get; set; }
        public List<float> OcclusionScores { get; set; }
        public List<float> CloudInlierScores { get; set; }
        public List<float> CorrespondenceInlierScores { get; set; }
        public List<int> SymmetryFilteredIds { get; set; }
        public List<int> SymmetryMergedIds { get; set; }
        public List<ISymmetry> MergedSymmetries { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private List<ISymmetry> InitialSymmetries { get; set; }
        private PointCloud Cloud { get; set; }
        private PCA PCA { get; set; }

        public float CalculateGlobalScore()
        {
            throw new NotImplementedException();
        }

        public void CalculateSymmetryPointSymmetryScores(PointCloud cloud, ISymmetry symmetry, out List<float> pointSymmetryScores, out List<Correspondence> correspondences)
        {
            throw new NotImplementedException();
        }

        public void Detect()
        {
            InitialSymmetries = GetInitialSymmetries(Cloud);
            //for (int symId = 0; symId < InitialSymmetries.Count; symId++)
            //{
            //    // Create optimization object
            //    sym::RotSymRefineFunctorDiff<PointT> functor;
            //    functor.cloud_ = Cloud;// cloud_no_boundary_;
            //    functor.max_fit_angle_ = REFERENCE_MAX_FIT_ANGLE;
            //    LevenbergMarquadtEJML lm = new LevenbergMarquadtEJML(functor);
            //    //lm.parameters.ftol = 1e-12;
            //    //lm.parameters.maxfev = 800;

            //    // Refine symmetry
            //    double[] x = new double[6];
            //    //{

            //    //};
            ////x.head(3) = symmetries_initial_[symId].getOrigin();
            ////x.tail(3) = symmetries_initial_[symId].getDirection();
            //var solution = lm.Minimise(x);
            //    var solutionSymmetry = new RotationalSymmetry(null, null);
            //symmetries_refined_[symId] = sym::RotationalSymmetry(x.head(3), x.tail(3));
            //symmetries_refined_[symId].setOriginProjected(cloud_mean_);

            //// Score symmetry
            //symmetry_scores_[symId] = sym::rotSymCloudSymmetryScore<PointT>(*cloud_no_boundary_,
            //                                                                             symmetries_refined_[symId],
            //                                                                             point_symmetry_scores_[symId],
            //                                                                             params_.min_normal_fit_angle,
            //                                                                             params_.max_normal_fit_angle);
            //occlusion_scores_[symId] = sym::rotSymCloudOcclusionScore<PointT>(*cloud_,
            //                                                                              occupancy_map_,
            //                                                                              symmetries_refined_[symId],
            //                                                                              point_occlusion_scores_[symId],
            //                                                                              params_.min_occlusion_distance,
            //                                                                              params_.max_occlusion_distance);
            //perpendicular_scores_[symId] = sym::rotSymCloudPerpendicularScores<PointT>(*cloud_no_boundary_,
            //                                                                              symmetries_refined_[symId],
            //                                                                              point_perpendicular_scores_[symId]);

            //coverage_scores_[symId] = sym::rotSymCloudCoverageAngle<PointT>(*cloud_,
            //                                                                              symmetries_refined_[symId]);
            //coverage_scores_[symId] /= (M_PI * 2);
            //}

        }

        public void Filter()
        {
            SymmetryFilteredIds = new List<int>();

            //for (int symId = 0; symId < SymmetriesRefined.Count; symId++)
            //{
            //    // Check if it's a good symmetry
            //    if (symmetry_scores_[symId] < MAX_SYMMETRY_SCORE && occlusion_scores_[symId] < MAX_OCCLUSION_SCORE &&  perpendicular_scores_[symId] < MAX_PERPENDICULAR_SCORE && coverage_scores_[symId] > MIN_COVERAGE_SCORE)
            //    {
            //        SymmetryFilteredIds.Add(symId);
            //    }
            //}
        }

        public List<ISymmetry> GetInitialSymmetries(PointCloud cloud)
        {
            List<ISymmetry> syms = new List<ISymmetry>();
            var mean = PCA.Mean;
            syms.Add(new RotationalSymmetry(mean, Vector3.UnitX));
            syms.Add(new RotationalSymmetry(mean, Vector3.UnitY));
            syms.Add(new RotationalSymmetry(mean, Vector3.UnitZ));
            return syms;
        }

        public void Merge()
        {
            //float bestOcclusionScore = float.MaxValue;
            //int bestSymId = -1;
            //for (int symIdIt = 0; symIdIt < SymmetryFilteredIds.Count; symIdIt++)
            //{
            //    int symId = SymmetryFilteredIds[symIdIt];
            //    if (occlusion_scores_[symId] < bestOcclusionScore)
            //    {
            //        bestSymId = symId;
            //        bestOcclusionScore = occlusion_scores_[symId];
            //    }
            //}

            //if (bestSymId != -1)
            //    SymmetryMergedIds.Add(bestSymId);
        }

        public List<int> MergeDuplicateSymmetries(List<ISymmetry> symmetries, List<int> indices, List<Vector3> symmetryReferencePoints, List<float> occlusionScores)
        {
            throw new NotImplementedException();
        }

        public List<ISymmetry> MergeDuplicateSymmetries(List<ISymmetry> filteredSymmetries, List<Vector3> symmetryReferencePoints, List<float> occlusionScores)
        {
            throw new NotImplementedException();
        }

        public List<ISymmetry> MergeDuplicateSymmetries(List<ISymmetry> filteredSymmetries, List<Vector3> symmetryReferencePoints)
        {
            throw new NotImplementedException();
        }

        public bool RefineGlobalSymmetryPosition(PointCloud cloud, ISymmetry symmetry)
        {
            throw new NotImplementedException();
        }

        public ISymmetry RefineSymmetryPosition(PointCloud cloud, ISymmetry originalSymmetry)
        {
            throw new NotImplementedException();
        }

        public void SetCloud(PointCloud cloud)
        {
            this.Cloud = cloud;
        }

        public void SetPCA(PCA pca)
        {
            this.PCA = pca;
        }
    }
}

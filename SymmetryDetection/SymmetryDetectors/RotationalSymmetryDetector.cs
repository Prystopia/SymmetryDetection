using SymmetryDetection.DataTypes;
using SymmetryDetection.Interfaces;
using SymmetryDetection.Optimisation;
using SymmetryDetection.Parameters;
using SymmetryDetection.Refinement;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDetectors
{
    public class RotationalSymmetryDetector : ISymmetryDetector
    {
        public List<ISymmetry> RefinedSymmetries { get; set; }
        public List<float> OcclusionScores { get; set; }
        public List<float> CloudInlierScores { get; set; }
        public List<float> CorrespondenceInlierScores { get; set; }
        public List<ISymmetry> FilteredSymmetries { get; set; }
        public List<ISymmetry> MergedSymmetries { get; set; }

        public int MaxPlanes => 3;

        private List<ISymmetry> InitialSymmetries { get; set; }
        private PointCloud Cloud { get; set; }
        private PCA PCA { get; set; }

        private ISymmetryScoreService RotationalScoreService { get; set; }
        private ISymmetryScoreService PerpendicularScoreService { get; set; }
        private ISymmetryScoreService CoverageScoreService { get; set; }

        public RotationalSymmetryDetector(ISymmetryScoreService rotationalScoreService, ISymmetryScoreService perpendicularScoreService, ISymmetryScoreService coverageScoreService)
        {
            this.RotationalScoreService = rotationalScoreService;
            this.PerpendicularScoreService = perpendicularScoreService;
            this.CoverageScoreService = coverageScoreService;
        }

        public float CalculateGlobalSymmetryScore()
        {
            throw new NotImplementedException();
        }

        public void Detect()
        {
            InitialSymmetries = GetInitialSymmetries(Cloud);
            
            foreach (var symmetry in InitialSymmetries)
            {
                RotationalSymmetryRefinementFunction functor = new RotationalSymmetryRefinementFunction();
                functor.Cloud = Cloud;
                functor.MaxFitAngle = RotationalSymmetryParameters.REFERENCE_MAX_FIT_ANGLE;
                LinearSolverHouseHolder linearSolver = new LinearSolverHouseHolder(new AlglibQRDecomposer());
                LevenbergMarquadtEJML lm = new LevenbergMarquadtEJML(functor, linearSolver);
                double[] symmDetails = new double[6]
                {
                    symmetry.Origin.X,
                    symmetry.Origin.Y,
                    symmetry.Origin.Z,
                    symmetry.Normal.X,
                    symmetry.Normal.Y,
                    symmetry.Normal.Z,
                };
                var solution = lm.Minimise(symmDetails);
                var solutionSymmetry = new RotationalSymmetry(new Vector3((float)solution[0], (float)solution[1], (float)solution[2]), new Vector3((float)solution[3], (float)solution[4], (float)solution[5]));
                solutionSymmetry.SetOriginProjected(Cloud.Mean);

                symmetry.SymmetryScore = RotationalScoreService.CalculateSymmetryPointSymmetryScores(Cloud, symmetry, false, out _)[0];
                symmetry.OcclusionScore = 0;
                ((RotationalSymmetry)symmetry).PerpendicularScore = PerpendicularScoreService.CalculateSymmetryPointSymmetryScores(Cloud, symmetry, false, out _)[0];
                ((RotationalSymmetry)symmetry).CoverageScore = CoverageScoreService.CalculateSymmetryPointSymmetryScores(Cloud, symmetry, false, out _)[0] / (MathF.PI * 2);
            }
        }

        public void Filter()
        {
            FilteredSymmetries = new List<ISymmetry>();

            foreach(ISymmetry symmetry in RefinedSymmetries)
            {
                RotationalSymmetry castSymmetry = (RotationalSymmetry)symmetry;

                if (castSymmetry.SymmetryScore < RotationalSymmetryParameters.MAX_SYMMETRY_SCORE &&
                    castSymmetry.OcclusionScore < RotationalSymmetryParameters.MAX_OCCLUSION_SCORE &&
                    castSymmetry.PerpendicularScore < RotationalSymmetryParameters.MAX_PERPENDICULAR_SCORE &&
                    castSymmetry.CoverageScore > RotationalSymmetryParameters.MIN_COVERAGE_SCORE)
                {
                    FilteredSymmetries.Add(symmetry);
                }
            }
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
            //original works based off occlusion scores, however we're not checking those so don't perform any merge for now
            foreach(var symmetry in FilteredSymmetries)
            {
                MergedSymmetries.Add(symmetry);
            }
        }

        public List<ISymmetry> MergeDuplicateSymmetries(List<ISymmetry> symmetries, List<Vector3> symmetryReferencePoints)
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

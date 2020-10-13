using SymmetryDetection.Clustering;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Enums;
using SymmetryDetection.Extensions;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.Optimisation;
using SymmetryDetection.Parameters;
using SymmetryDetection.Refinement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class ReflectionalSymmetryDetector : ISymmetryDetector
    {
        public SymmetryTypeEnum SymmetryType => SymmetryTypeEnum.Reflectional;
        public PointCloud Cloud { get; set; }
        public PCA PCA { get; set; }
        public List<ISymmetry> MergedSymmetries { get; set; }

        private List<Correspondence> Correspondences { get; set; }
        private List<ISymmetry> InitialSymmetries { get; set; }
        private List<ISymmetry> RefinedSymmetries { get; set; }
        private List<ISymmetry> FilteredSymmetries { get; set; }
        private IScoreService<ReflectionalSymmetryParameters> ReflectionalScoreService { get; set; }
        private ReflectionalSymmetryParameters Parameters { get; set; }

        public ReflectionalSymmetryDetector(IScoreService<ReflectionalSymmetryParameters> reflectionalScoreService, ReflectionalSymmetryParameters parameters = null)
        {
            this.ReflectionalScoreService = reflectionalScoreService;
            this.Parameters = parameters ?? new ReflectionalSymmetryParameters();
        }

        public void SetCloud(PointCloud cloud)
        {
            this.Cloud = cloud;
        }
        public void SetPCA(PCA pca)
        {
            this.PCA = pca;
        }

        public void Detect()
        {
            RefinedSymmetries = new List<ISymmetry>();
            Correspondences = new List<Correspondence>();

            GetInitialSymmetries(Cloud);

            foreach(var currentSymmetry in InitialSymmetries)
            {
                List<Correspondence> currentCorrespondances = new List<Correspondence>();
                ISymmetry refined = RefineSymmetryPosition(Cloud, currentSymmetry);
                if (RefineGlobalSymmetryPosition(Cloud, refined))
                {
                    List<float> currentPointSymmetryScores = new List<float>();

                    this.ReflectionalScoreService.CalculateSymmetryPointSymmetryScores(Cloud, currentSymmetry, false, Parameters, out List<float> symmScores, out List<Correspondence> symCorrespondences);
                    currentCorrespondances.AddRange(symCorrespondences);
                    currentPointSymmetryScores.AddRange(symmScores);

                    float inlierScoreSum = 0;

                    for (int k = 0; k < currentCorrespondances.Count; k++)
                    {
                        inlierScoreSum += (1 - currentPointSymmetryScores[k]);
                    }


                    //symmetry score is the average error for each point in the cloud
                    refined.SymmetryScore = (inlierScoreSum / Cloud.Points.Count);
                    ((ReflectionalSymmetry)refined).CorrespondenceInlierScore = inlierScoreSum / currentCorrespondances.Count;
                    refined.SymmetryType = this.SymmetryType;
                    RefinedSymmetries.Add(refined);

                    Correspondences.Add(currentCorrespondances[0]);
                }
            }
        }

        public void Filter()
        {
            FilteredSymmetries = new List<ISymmetry>();
            foreach(ReflectionalSymmetry symmetry in RefinedSymmetries)
            {
                if (symmetry.SymmetryScore > Parameters.MIN_CLOUD_INLIER_SCORE && symmetry.CorrespondenceInlierScore > Parameters.MIN_CORRESPONDANCE_INLIER_SCORE)
                {
                    FilteredSymmetries.Add(symmetry);
                }
            }
        }
        public void Merge()
        {
            MergedSymmetries = new List<ISymmetry>();

            var symmetryAdjacency = InitialiseSimalarityGraph();
            
            BronKerbosch bk = new BronKerbosch(symmetryAdjacency);
            List<IList<int>> cliques = bk.RunAlgorithm(1);
            List<IList<int>> hypothesisClusters = new List<IList<int>>();
            while (cliques.Count > 0)
            {
                IList<int> largestClique = null;
                int maxCliqueSize = 0;
                foreach (var clique in cliques)
                {
                    if (clique.Count > maxCliqueSize)
                    {
                        largestClique = clique;
                        maxCliqueSize = clique.Count;
                    }
                }

                // Add hypotheses from the largest clique to the list of hypothesis clusters
                hypothesisClusters.Add(largestClique);
                cliques.Remove(largestClique);

                // Remove hyptheses belonging to the largest clique from remaining cliques
                List<IList<int>> itemsToRemove = new List<IList<int>>();
                foreach (var clique in cliques)
                {
                    foreach (var cluster in hypothesisClusters)
                    {
                        foreach (var val in cluster)
                        {
                            clique.Remove(val);
                        }
                        if (clique.Count <= 0)
                        {
                            //remove the clique
                            itemsToRemove.Add(clique);
                        }
                    }
                }
                foreach (var item in itemsToRemove)
                {
                    cliques.Remove(item);
                }
            }
            foreach (var cluster in hypothesisClusters)
            {
                float minScore = float.MaxValue;
                ISymmetry bestHypothesis = null;
                for (int i = 0; i < cluster.Count; i++)
                {
                    ISymmetry hypothesis = FilteredSymmetries[cluster[i]];
                    if (hypothesis.SymmetryScore < minScore)
                    {
                        minScore = hypothesis.SymmetryScore;
                        bestHypothesis = hypothesis;
                    }
                }
                MergedSymmetries.Add(bestHypothesis);
            }
        }
        public float CalculateGlobalScore()
        {
            float inlierScoreSum = 0;
            foreach (ReflectionalSymmetry symmetry in MergedSymmetries)
            {
                inlierScoreSum += symmetry.SymmetryScore;
            }
            //average error for all points across all detected symmetries;
            return inlierScoreSum / (MergedSymmetries.Count > 0 ? MergedSymmetries.Count : 1);
        }

        private Graph InitialiseSimalarityGraph()
        {
            Graph symmetryAdjacency = new Graph(FilteredSymmetries.Count);

            for (int i = 0; i < FilteredSymmetries.Count; i++)
            {
                var srcHypothesis = FilteredSymmetries[i];
                Vector3 srcReferencePoint = PCA.Mean;
                for (int j = i + 1; j < FilteredSymmetries.Count; j++)
                {
                    var targetHypothesis = FilteredSymmetries[j];
                    Vector3 targetReferencePoint = PCA.Mean;
                    if (Parameters.MAX_REFERENCE_POINT_DISTANCE <= 0 || MathsHelpers.PointToPointDistance(srcReferencePoint, targetReferencePoint) <= Parameters.MAX_REFERENCE_POINT_DISTANCE)
                    {
                        var referencePoint = (srcReferencePoint + targetReferencePoint) / 2f;
                        PointHelpers.ReflectedSymmetryDifference(targetHypothesis, referencePoint, srcHypothesis, out float angleDiff, out float distanceDiff);
                        if (angleDiff < Parameters.MAX_NORMAL_ANGLE_DIFF && distanceDiff < Parameters.MAX_DISTANCE_DIFF)
                        {
                            symmetryAdjacency.AddEdge(i, j);
                        }
                    }
                }
            }
            return symmetryAdjacency;
        }

        private void GetInitialSymmetries(PointCloud cloud)
        {
            InitialSymmetries = new List<ISymmetry>();
            //use a step every 45 degrees 360/45 = 8 
            List<Vector3> spherePoints = GenerateSpherePoints(8);

            // Convert to symmetries (and rotate normals using major axes)
            foreach (var point in spherePoints)
            {
                InitialSymmetries.Add(new ReflectionalSymmetry(PCA.Mean, point));
            }
        }

        /// <summary>
        /// Place a point at specified intervals around a unit sphere, these will act as the initial symmetry checks
        /// </summary>
        /// <param name="numSegments">Total segments to split the sphere into</param>
        /// <returns>A list of points around the sphere</returns>
        private List<Vector3> GenerateSpherePoints(int numSegments)
        {
            List<Vector3> spherePoints = new List<Vector3>();
            if (numSegments <= 1)
            {
                throw new Exception("Number of Divisions must be greater than 1");
            }

            //angle along x-axis - radians
            float azimuthalAngleStepRad = (2 * MathF.PI) / numSegments;

            //add the polar point
            spherePoints.Add(Vector3.UnitZ);

            //angle along z-axis - radians
            var polarAngleStepRad = MathF.PI / (numSegments);

            for (int i = 0; i < numSegments; i++)
            {
                //start at 1 to skip the polar point duplication
                for (int j = 1; j < numSegments; j++)
                {
                    Vector3 point = new Vector3();
                    point.X = MathF.Sin(polarAngleStepRad * j) * MathF.Cos(azimuthalAngleStepRad * i);
                    point.Y = MathF.Sin(polarAngleStepRad * j) * MathF.Sin(azimuthalAngleStepRad * i);
                    point.Z = MathF.Cos(polarAngleStepRad * j);
                    spherePoints.Add(point);
                }
            }
            return spherePoints;
        }

        private ISymmetry RefineSymmetryPosition(PointCloud cloud, ISymmetry originalSymmetry)
        {
            ISymmetry refined = originalSymmetry;
            List<Correspondence> correspondences = new List<Correspondence>();

            var origin = refined.Origin;
            var normal = refined.Normal;

            PointCloud cloudProjected = PointCloudHelpers.ProjectCloudToPlane(cloud, origin, normal);
            for (int i = 0; i < cloud.Points.Count; i++)
            {
                var projectedPoint = cloudProjected.Points[i];
                var originalPoint = cloud.Points[i];

                var srcPoint = originalPoint.Position;
                var neighbours = cloudProjected.GetNeighbours(projectedPoint, 5);

                float minimumFitError = float.MaxValue;
                float minimumReflectedDistance = float.MaxValue;
                PointXYZNormal bestFit = null;

                foreach (var neighbour in neighbours)
                {
                    var neighbourPoint = cloud.Points[neighbour.index];
                    var targetPos = neighbourPoint.Position;

                    //check distances between points and plane
                    var origDistance = PointHelpers.PointSignedDistance(srcPoint, originalSymmetry);
                    var neighbourDistance = PointHelpers.PointSignedDistance(targetPos, originalSymmetry);

                    // If the distance along the symmetry normal between the points of a symmetric correspondence is too small - reject
                    if (MathF.Abs(origDistance - neighbourDistance) < Parameters.MIN_SYMMETRY_CORRESPONDENCE_DISTANCE)
                    {
                        continue;
                    }

                    //issue is we're getting false positives with a sym fit error of 0 which aren't correspondences as the plane lies halfway between the points
                    //work around this by checking the distance between the original point and reflected target point
                    var reflectedTargetPos = PointHelpers.ReflectPoint(targetPos, originalSymmetry);
                    float reflectedDistance = originalPoint.GetDistance(reflectedTargetPos);

                    float symFitError = MathF.Abs(ReflectionHelpers.GetReflectionSymmetryPositionFitError(srcPoint, targetPos, refined));
                    if (reflectedDistance <= Parameters.MIN_SYMMETRY_CORRESPONDENCE_DISTANCE)
                    {
                        //the closest reflected point with the minimum symmetry error
                        if (symFitError <= minimumFitError && reflectedDistance <= minimumReflectedDistance)
                        {
                            minimumFitError = symFitError;
                            minimumReflectedDistance = reflectedDistance;
                            bestFit = neighbourPoint;
                        }
                    }

                }
                if (bestFit != null)
                {
                    correspondences.Add(new Correspondence(originalPoint, bestFit, minimumFitError));
                }
            }

            CorrespondenceRejector rejector = new CorrespondenceRejector();
            correspondences = rejector.GetRemainingCorrespondences(correspondences);

            if (correspondences.Count > 0)
            {
                List<float> positionFitErrors = new List<float>();
                foreach (var correspondance in correspondences)
                {
                    var src = correspondance.Original.Position;
                    var target = correspondance.CorrespondingPoint.Position;
                    //how close the midpoint of the correspondences is to the symmetry plane
                    positionFitErrors.Add(ReflectionHelpers.GetReflectionSymmetryPositionFitError(src, target, originalSymmetry));
                }

                float medianPositionFitError = MathsHelpers.Median(positionFitErrors);
                refined.Origin = origin + normal * medianPositionFitError;
            }

            return refined;
        }

        private bool RefineGlobalSymmetryPosition(PointCloud cloud, ISymmetry symmetry)
        {
            bool success = true;
            ISymmetry refinedSymmetry = symmetry;
            ISymmetry previousSymmetry;
            List<Correspondence> correspondences;
            int numIterations = 0;

            CorrespondenceRejector rejector = new CorrespondenceRejector();
            ReflectionalSymmetryRefinementFunction functor = new ReflectionalSymmetryRefinementFunction(cloud);

            bool done = false;
            while (!done)
            {
                previousSymmetry = refinedSymmetry;
                correspondences = new List<Correspondence>();
                Vector3 symmetryOrigin = refinedSymmetry.Origin;
                Vector3 symmetryNormal = refinedSymmetry.Normal;

                for (int i = 0; i < cloud.Points.Count; i++)
                {
                    var point = cloud.Points[i];

                    Vector3 srcPoint = point.Position;
                    Vector3 srcNormal = point.Normal;

                    Vector3 srcPointReflected = PointHelpers.ReflectPoint(srcPoint, refinedSymmetry);
                    Vector3 srcNormalReflected = PointHelpers.ReflectNormal(srcNormal, refinedSymmetry);

                    PointXYZNormal searchPoint = new PointXYZNormal()
                    {
                        Position = srcPointReflected,
                        Normal = srcNormalReflected
                    };

                    var neighbours = cloud.GetClosetNeighbours(searchPoint, 1);

                    Vector3 targetPoint = neighbours[0].neighbour.Position;

                    // NOTE: this is required for faster convergence. Distance along symmetry
                    // normal works faster than point to point distnace
                    // If the distance along the symmetry normal between the points of a symmetric correspondence is too small - reject
                    if (MathF.Abs(PointHelpers.PointSignedDistance(srcPoint, symmetry) - PointHelpers.PointSignedDistance(targetPoint, symmetry)) >= Parameters.MIN_SYMMETRY_CORRESPONDENCE_DISTANCE)
                    {
                        // If the distance between the reflected source point and it's nearest neighbor is too big - reject
                        if (neighbours[0].distance <= Parameters.MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE)
                        {
                            // Reject correspondence if normal error is too high - not so fussed about the normal error
                            //float error = GetReflectionSymmetryNormalFitError(srcNormal, targetNormal, refinedSymmetry);
                            //if(error <= MAX_SYMMETRY_NORMAL_FIT_ERROR)
                            {
                                correspondences.Add(new Correspondence(point, neighbours[0].neighbour, neighbours[0].distance));
                            }
                        }
                    }

                }
                correspondences = rejector.GetRemainingCorrespondences(correspondences);
                if (correspondences.Count > 0)
                {
                    double[] input = new double[6]
                    {
                        symmetryOrigin.X,
                        symmetryOrigin.Y,
                        symmetryOrigin.Z,
                        symmetryNormal.X,
                        symmetryNormal.Y,
                        symmetryNormal.Z,
                    };
                    functor.Correspondences = correspondences;

                    IDecompositionHandler decompositionHandler = new AlglibQRDecomposer();
                    ILinearSolver linearSolver = new LinearSolverHouseHolder(decompositionHandler);
                    LevenbergMarquadtEJML lm = new LevenbergMarquadtEJML(functor, linearSolver);
                    var solution = lm.Minimise(input);
                    //parameters are the correspondences
                    Vector3 origin = new Vector3((float)solution[0], (float)solution[1], (float)solution[2]);
                    Vector3 normal = new Vector3((float)solution[3], (float)solution[4], (float)solution[5]);

                    refinedSymmetry = new ReflectionalSymmetry(origin, normal);
                    refinedSymmetry.SetOriginProjected(PCA.Mean); // seems to be overwriting the above line by setting origin to the 

                    if (++numIterations >= Parameters.MAX_ITERATIONS)
                    {
                        done = true;
                    }
                    PointHelpers.ReflectedSymmetryDifference(previousSymmetry, PCA.Mean, refinedSymmetry, out float angleDiff, out float distanceDiff);

                    if (angleDiff < (0.05f).ConvertToRadians() && distanceDiff < 0.0001f)
                    {
                        done = true;
                    }
                }
                else
                {
                    done = true;
                    success = false;
                }
            }
            return success;
        }
    }
}

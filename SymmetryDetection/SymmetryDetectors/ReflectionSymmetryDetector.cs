
using SymmetryDetection.Clustering;
using SymmetryDetection.DataTypes;
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
    public class ReflectionalSymmetryDetector : ISymmetryDetector<ReflectionalSymmetry>
    {
        public int MaxPlanes => 57;
        public PointCloud Cloud { get; set; }
        public PCA PCA { get; set; }
        public Vector3 CloudMean { get; set; }
        //public List<Correspondence> Correspondences { get; set; }
        public List<ReflectionalSymmetry> SymmetriesInitial { get; set; }
        public List<ReflectionalSymmetry> RefinedSymmetries { get; set; }
        public List<ReflectionalSymmetry> FilteredSymmetries { get; set; }
        public List<ReflectionalSymmetry> MergedSymmetries { get; set; }

        private ISymmetryScoreService ScoreService { get; set; }

        public ReflectionalSymmetryDetector(ISymmetryScoreService scoreService)
        {
            this.ScoreService = scoreService;
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
            CloudMean = Vector3.Zero;
            RefinedSymmetries = new List<ReflectionalSymmetry>();
            //Correspondences = new List<Correspondence>();

            SymmetriesInitial = GetInitialSymmetries(Cloud);

            for (int j = 0; j < SymmetriesInitial.Count; j++)
            {
                ReflectionalSymmetry currentSymmetry = SymmetriesInitial[j];
                //List<Correspondence> currentCorrespondances = new List<Correspondence>();
                var refined = RefineSymmetryPosition(Cloud, currentSymmetry);
                if (RefineGlobalSymmetryPosition(Cloud, refined))
                {
                    List<float> currentPointSymmetryScores = this.ScoreService.CalculateSymmetryPointSymmetryScores(Cloud, refined, false, out List<Correspondence> symCorrespondences);
                    //currentCorrespondances.AddRange(symCorrespondences);

                    float inlierScoreSum = 0;

                    for (int k = 0; k < symCorrespondences.Count; k++)
                    {
                        inlierScoreSum += (1 - currentPointSymmetryScores[k]);
                    }

                    float currentCloudInlierScore = inlierScoreSum / Cloud.Points.Count;
                    float currentCorrespondanceInlierScore = inlierScoreSum / symCorrespondences.Count;

                    refined.OcclusionScore = 0;
                    refined.InlierScore = currentCloudInlierScore;
                    refined.CorrespondenceInlierScore = currentCorrespondanceInlierScore;
                    RefinedSymmetries.Add(currentSymmetry);
                    //Correspondences.Add(symCorrespondences[0]);

                    //pointSymmetryScoresTMP.Add(currentPointSymmetryScores);
                    //validSymTableTMP.Add(true);
                    //symmetryCorrespTMP.AddRange(currentCorrespondances);
                }
            }

            //for (int j = 0; j < validSymTableTMP.Count; j++)
            //{
            //    bool validSym = validSymTableTMP[j];
            //    if (validSym)
            //    {
            //        RefinedSymmetries.Add(symmetriesTMP[j]);
            //        CloudInlierScores.Add(cloudInlierScoresTMP[j]);
            //        CorrespondenceInlierScores.Add(correspInlierScoresTMP[j]);
            //        PointSymmetryScores.Add(pointSymmetryScoresTMP[j]);
            //        Correspondences.Add(symmetryCorrespTMP[j]);
            //    }
            //}
        }

        public void Filter()
        {
            FilteredSymmetries = new List<ReflectionalSymmetry>();
            foreach(var sym in RefinedSymmetries)
            {
                if(sym.OcclusionScore < ReflectionalSymmetryParameters.MAX_OCCLUSION_SCORE &&
                    sym.InlierScore > ReflectionalSymmetryParameters.MIN_CLOUD_INLIER_SCORE && 
                    sym.CorrespondenceInlierScore > ReflectionalSymmetryParameters.MIN_CORRESPONDANCE_INLIER_SCORE)
                {
                    FilteredSymmetries.Add(sym);
                }
            }
        }

        public void Merge()
        {
            MergedSymmetries = new List<ReflectionalSymmetry>();
            List<Vector3> referencePoints = new List<Vector3>();
            for (int i = 0; i < FilteredSymmetries.Count; i++)
            {
                referencePoints.Add(CloudMean);
            }
            if (referencePoints.Any())
            {
                MergedSymmetries = MergeDuplicateSymmetries(FilteredSymmetries, referencePoints);
            }
        }

        public List<ReflectionalSymmetry> MergeDuplicateSymmetries(List<ReflectionalSymmetry> symmetries, List<Vector3> symmetryReferencePoints)
        {
            List<ReflectionalSymmetry> mergedSymmetries = new List<ReflectionalSymmetry>();
            Graph symmetryAdjacency = new Graph(symmetries.Count);

            for (int i = 0; i < symmetries.Count; i++)
            {
                var srcHypothesis = symmetries[i];
                Vector3 srcReferencePoint = symmetryReferencePoints[i];

                for (int j = i + 1; j < symmetries.Count; j++)
                {
                    var targetHypothesis = symmetries[j];
                    Vector3 targetReferencePoint = symmetryReferencePoints[j];
                    if (ReflectionalSymmetryParameters.MAX_REFERENCE_POINT_DISTANCE <= 0 || MathsHelpers.PointToPointDistance(srcReferencePoint, targetReferencePoint) <= ReflectionalSymmetryParameters.MAX_REFERENCE_POINT_DISTANCE)
                    {
                        var referencePoint = (srcReferencePoint + targetReferencePoint) / 2f;
                        PointHelpers.ReflectedSymmetryDifference(targetHypothesis, referencePoint, srcHypothesis, out float angleDiff, out float distanceDiff);
                        if (angleDiff < ReflectionalSymmetryParameters.MAX_NORMAL_ANGLE_DIFF && distanceDiff < ReflectionalSymmetryParameters.MAX_DISTANCE_DIFF)
                        {
                            symmetryAdjacency.AddEdge(i, j);
                        }
                    }
                }
            }
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
                ReflectionalSymmetry bestHypothesis = null;
                for (int i = 0; i < cluster.Count; i++)
                {
                    var hypothesis = symmetries[cluster[i]];
                    if (hypothesis.OcclusionScore < minScore)
                    {
                        minScore = hypothesis.OcclusionScore;
                        bestHypothesis = hypothesis;
                    }
                }
                mergedSymmetries.Add(bestHypothesis);
            }
            return mergedSymmetries;
        }

        public List<ReflectionalSymmetry> GetInitialSymmetries(PointCloud cloud)
        {
            List<ReflectionalSymmetry> symmetries = new List<ReflectionalSymmetry>();
            //use a step every 45 degrees 360/45 = 8 
            List<Vector3> spherePoints = GenerateSpherePoints(8);

            // Convert to symmetries (and rotate normals using major axes)
            foreach (var point in spherePoints)
            {
                symmetries.Add(new ReflectionalSymmetry(PCA.Mean, point));
            }

            return symmetries;
        }

        /// <summary>
        /// Place a point at specified intervals around a unit sphere, these will act as the initial symmetry checks
        /// </summary>
        /// <param name="numSegments">Total segments to split the sphere into</param>
        /// <returns>A list of points around the sphere</returns>
        public List<Vector3> GenerateSpherePoints(int numSegments)
        {
            List<Vector3> spherePoints = new List<Vector3>();
            if (numSegments <= 1)
            {
                throw new Exception("Number of Divisions must be greater than 1");
            }

            //angle along x-axis - radians
            float azimuthalAngleStepRad  = (2 * MathF.PI) /  numSegments;

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

        public ReflectionalSymmetry RefineSymmetryPosition(PointCloud cloud, ReflectionalSymmetry originalSymmetry)
        {
            ReflectionalSymmetry refined = originalSymmetry;
            List<Correspondence> correspondences = new List<Correspondence>();
            //const float minimumSymmetryCorrespondanceDistance = 0.02f;
            //float maxSymmetryNormalFitError = (10f).ConvertToRadians();

            var origin = refined.Origin;
            var normal = refined.Normal;

            PointCloud cloudProjected = PointCloudHelpers.ProjectCloudToPlane(cloud, origin, normal);
            for (int i = 0; i < cloud.Points.Count; i++)
            {
                var projectedPoint = cloudProjected.Points[i];
                var originalPoint = cloud.Points[i];

                var srcPoint = originalPoint.Position;
               // var srcNormal = originalPoint.Normal;
                var neighbours = cloudProjected.GetNeighbours(projectedPoint, 5);

                float minimumFitError = float.MaxValue;
                float minimumReflectedDistance = float.MaxValue;
                PointXYZRGBNormal bestFit = null;

                foreach (var neighbour in neighbours)
                {
                    var neighbourPoint = cloud.Points[neighbour.index];
                    var targetPos = neighbourPoint.Position;
                    //var targetNormal = neighbourPoint.Normal;

                    //check distances between points and plane
                    var origDistance = PointHelpers.PointSignedDistance(srcPoint, originalSymmetry);
                    var neighbourDistance = PointHelpers.PointSignedDistance(targetPos, originalSymmetry);
                    //if (PerformNormalValidation)
                    {
                        // If the distance along the symmetry normal between the points of a symmetric correspondence is too small - reject
                        if (MathF.Abs(origDistance - neighbourDistance) < ReflectionalSymmetryParameters.MIN_SYMMETRY_CORRESPONDENCE_DISTANCE)
                        {
                            continue;
                        }

                        //check the distance between the reflected points
                        //reflect target check distance

                        //issue is we're getting false positives with a sym fit error of 0 which aren't correspondences as the plane lies halfway between the points
                        //work around this by checking the distance between the original point and reflected target point
                        var reflectedTargetPos = PointHelpers.ReflectPoint(targetPos, originalSymmetry);
                        float reflectedDistance = originalPoint.GetDistance(reflectedTargetPos);

                        float symFitError = MathF.Abs(ReflectionHelpers.GetReflectionSymmetryPositionFitError(srcPoint, targetPos, refined));
                        //if (reflectedDistance <= MIN_SYMMETRY_CORRESPONDENCE_DISTANCE)
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

                float medianPositionFitError = MathsHelpers.Median(positionFitErrors.ToArray());
                refined.Origin = origin + normal * medianPositionFitError;
            }

            return refined;
        }

        public bool RefineGlobalSymmetryPosition(PointCloud cloud, ReflectionalSymmetry symmetry)
        {
            bool success = true;
            ReflectionalSymmetry refinedSymmetry = symmetry;
            ReflectionalSymmetry previousSymmetry;
            List<Correspondence> correspondences;
            int numIterations = 0;

            CorrespondenceRejector rejector = new CorrespondenceRejector();
            ReflectionalSymmetryRefinementFunction functor = new ReflectionalSymmetryRefinementFunction(cloud);
            
            bool done = false;
            while(!done)
            {
                previousSymmetry = refinedSymmetry;
                correspondences = new List<Correspondence>();
                Vector3 symmetryOrigin = refinedSymmetry.Origin;
                Vector3 symmetryNormal = refinedSymmetry.Normal;

                for(int i = 0; i < cloud.Points.Count; i++)
                {
                    var point = cloud.Points[i];

                    Vector3 srcPoint = point.Position;
                    Vector3 srcNormal = point.Normal;

                    Vector3 srcPointReflected = PointHelpers.ReflectPoint(srcPoint, refinedSymmetry);
                    Vector3 srcNormalReflected = PointHelpers.ReflectNormal(srcNormal, refinedSymmetry);
                    PointXYZRGBNormal searchPoint = new PointXYZRGBNormal()
                    {
                        Position = srcPointReflected,
                        Normal = srcNormalReflected
                    };

                    var neighbours = cloud.GetClosetNeighbours(searchPoint, 1);

                    Vector3 targetPoint = neighbours[0].neighbour.Position;

                    // NOTE: this is required for faster convergence. Distance along symmetry
                    // normal works faster than point to point distnace
                    // If the distance along the symmetry normal between the points of a symmetric correspondence is too small - reject
                    if (MathF.Abs(PointHelpers.PointSignedDistance(srcPoint, symmetry) - PointHelpers.PointSignedDistance(targetPoint, symmetry)) >= ReflectionalSymmetryParameters.MIN_SYMMETRY_CORRESPONDENCE_DISTANCE)
                    {
                        // If the distance between the reflected source point and it's nearest neighbor is too big - reject
                        if (neighbours[0].distance <= ReflectionalSymmetryParameters.MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE)
                        {
                            correspondences.Add(new Correspondence(point, neighbours[0].neighbour, neighbours[0].distance));
                        }
                    }

                }
                correspondences = rejector.GetRemainingCorrespondences(correspondences);
                if(correspondences.Count > 0)
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

                    Vector6 solutionVector = new Vector6()
                    {
                        X = (float)solution[0],
                        Y = (float)solution[1],
                        Z = (float)solution[2],
                        A = (float)solution[3],
                        B = (float)solution[4],
                        C = (float)solution[5],
                    };
                    refinedSymmetry = new ReflectionalSymmetry(solutionVector.Head, solutionVector.Tail);
                    refinedSymmetry.SetOriginProjected(cloud.Mean);

                    if(++numIterations >= ReflectionalSymmetryParameters.MAX_ITERATIONS)
                    {
                        done = true;
                    }
                    PointHelpers.ReflectedSymmetryDifference(previousSymmetry, cloud.Mean, refinedSymmetry, out float angleDiff, out float distanceDiff);
                    
                    if(angleDiff < (0.05f).ConvertToRadians() && distanceDiff < 0.0001f)
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
        public float CalculateGlobalSymmetryScore()
        {
            float inlierScoreSum = 0;
            foreach (var symmetryPlane in MergedSymmetries)
            {
                var pointScores = this.ScoreService.CalculateSymmetryPointSymmetryScores(Cloud, symmetryPlane, true, out _);
                var pointScoreError = pointScores.Select(s => 1 - s).Sum();
                inlierScoreSum += pointScoreError / Cloud.Points.Count;
            }
            //average error for all points across all detected symmetries;
            return inlierScoreSum / (MergedSymmetries.Count > 0 ? MergedSymmetries.Count : 1);
        }
    }
}

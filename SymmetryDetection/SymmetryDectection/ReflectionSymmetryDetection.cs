using Accord.Math.Optimization;
using Accord.Statistics;
using SymmetryDetection.Clustering;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.Optimisation;
using SymmetryDetection.Refinement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class ReflectionalSymmetryDetection
    {
        private const float MAX_OCCLUSION_SCORE = 0.01f;
        private const float MIN_CLOUD_INLIER_SCORE = 0.5f;
        private const float MIN_CORRESPONDANCE_INLIER_SCORE = 0.7f;
        private const float MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE = 0.01f;
        private const float MIN_SYMMETRY_CORRESPONDENCE_DISTANCE = 0.02f;
        private const int MAX_ITERATIONS = 20;
        public const float MAX_REFERENCE_POINT_DISTANCE = 0.3f;
        public const float MAX_NORMAL_ANGLE_DIFF = 7;
        public const float MAX_DISTANCE_DIFF = 0.01f;

        public PointCloud Cloud { get; set; }
        public Vector3 CloudMean { get; set; }
        public List<Correspondence> Correspondences { get; set; }
        public List<ReflectionalSymmetry> SymmetriesInitial { get; set; }
        public List<ReflectionalSymmetry> SymmetriesRefined { get; set; }
        public List<float> OcclusionScores { get; set; }
        public List<float> CloudInlierScores { get; set; }
        public List<float> CorrespondenceInlierScores { get; set; }
        public List<List<float>> PointSymmetryScores { get; set; }
        public List<List<float>> PointOcclusionScores { get; set; }
        public List<int> SymmetryFilteredIds { get; set; }
        public List<int> SymmetryMergedIds { get; set; }

        public ReflectionalSymmetryDetection(PointCloud cloud)
        {
            this.Cloud = cloud;
        }

        public void Detect()
        {
            CloudMean = Vector3.Zero;
            SymmetriesRefined = new List<ReflectionalSymmetry>();
            Correspondences = new List<Correspondence>();
            OcclusionScores = new List<float>();
            PointSymmetryScores = new List<List<float>>();
            CloudInlierScores = new List<float>();
            CorrespondenceInlierScores = new List<float>();
            PointOcclusionScores = new List<List<float>>();
            SymmetryFilteredIds = new List<int>();
            SymmetryMergedIds = new List<int>();

            SymmetriesInitial = GetInitialReflectionSymmetries(Cloud);

            List<ReflectionalSymmetry> symmetriesTMP = new List<ReflectionalSymmetry>();
            List<float> occlusionScoresTMP = new List<float>();
            List<float> cloudInlierScoresTMP = new List<float>();
            List<float> correspInlierScoresTMP = new List<float>();
            List<List<float>> pointSymmetryScoresTMP = new List<List<float>>();
            List<List<float>> pointOcclusionScoresTMP = new List<List<float>>();
            List<bool> validSymTableTMP = new List<bool>();
            //List<bool> filteredSymTableTMP = new List<bool>();  Doesn't seem to be used
            List<Correspondence> symmetryCorrespTMP = new List<Correspondence>();

            for (int j = 0; j < SymmetriesInitial.Count; j++)
            {
                ReflectionalSymmetry currentSymmetry = SymmetriesInitial[j];
                List<Correspondence> currentCorrespondances = new List<Correspondence>();
                if (RefineSymmetryPosition(Cloud, SymmetriesInitial[j]))
                {
                    if (RefineGlobalSymmetryPosition(Cloud, currentSymmetry))
                    {
                        List<float> currentPointSymmetryScores = new List<float>(), currentPointOcclusionScores = new List<float>();
                        float currentOcclusionScore = 0, currentCloudInlierScore = 0, currentCorrespondanceInlierScore = 0;

                        CalculateSymmetryPointSymmetryScores(Cloud, currentSymmetry, out List<float> symmScores, out List<Correspondence> symCorrespondences);
                        currentCorrespondances.AddRange(symCorrespondences);
                        currentPointSymmetryScores.AddRange(symmScores);

                        // If an occupancy map is available - calculate occlusion score. If not set occlusion scores to a value that will pass the occlusion filter. -- Occlusion map is not available
                        for (int i = 0; i < Cloud.Points.Count; i++)
                        {
                            currentPointOcclusionScores.Add(0);
                        }
                        
                        float inlierScoreSum = 0;

                        for (int k = 0; k < currentCorrespondances.Count; k++)
                        {
                            inlierScoreSum += (1 - currentPointSymmetryScores[k]);
                        }

                        currentCloudInlierScore = inlierScoreSum / Cloud.Points.Count;
                        currentCorrespondanceInlierScore = inlierScoreSum / currentCorrespondances.Count;

                        symmetriesTMP[j] = currentSymmetry;
                        occlusionScoresTMP[j] = currentOcclusionScore;
                        cloudInlierScoresTMP[j] = currentCloudInlierScore;
                        correspInlierScoresTMP[j] = currentCorrespondanceInlierScore;
                        pointSymmetryScoresTMP[j] = currentPointSymmetryScores;
                        pointOcclusionScoresTMP[j] = currentPointOcclusionScores;
                        validSymTableTMP[j] = true;
                        symmetryCorrespTMP.AddRange(currentCorrespondances);
                    }
                }

            }

            for (int j = 0; j < validSymTableTMP.Count; j++)
            {
                bool validSym = validSymTableTMP[j];
                if (validSym)
                {
                    SymmetriesRefined.Add(symmetriesTMP[j]);
                    OcclusionScores.Add(occlusionScoresTMP[j]);
                    CloudInlierScores.Add(cloudInlierScoresTMP[j]);
                    CorrespondenceInlierScores.Add(correspInlierScoresTMP[j]);
                    PointSymmetryScores.Add(pointSymmetryScoresTMP[j]);
                    PointOcclusionScores.Add(pointOcclusionScoresTMP[j]);
                    Correspondences.Add(symmetryCorrespTMP[j]);
                }
            }
        }

        public void Filter()
        {
            SymmetryFilteredIds = new List<int>();
            for (int i = 0; i < SymmetriesRefined.Count; i++)
            {
                if (OcclusionScores[i] < MAX_OCCLUSION_SCORE &&
                    CloudInlierScores[i] > MIN_CLOUD_INLIER_SCORE &&
                    CorrespondenceInlierScores[i] > MIN_CORRESPONDANCE_INLIER_SCORE)
                {
                    SymmetryFilteredIds.Add(i);
                }
            }
        }

        public void Merge()
        {
            List<Vector3> referencePoints = new List<Vector3>();
            for (int i = 0; i < SymmetriesRefined.Count; i++)
            {
                referencePoints.Add(CloudMean);
            }

            MergeDuplicateReflectedSymmetries(SymmetriesRefined, referencePoints, OcclusionScores);
        }

        public List<int> MergeDuplicateReflectedSymmetries(List<ReflectionalSymmetry> symmetries, List<Vector3> symmetryReferencePoints, List<float> occlusionScores)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < symmetries.Count; i++)
            {
                indices.Add(i);
            }
            List<int> mergedSymmetryIds = new List<int>();
            Graph symmetryAdjacency = new Graph(indices.Count);

            for (int i = 0; i < indices.Count; i++)
            {
                int srcId = indices[i];
                ReflectionalSymmetry srcHypothesis = symmetries[srcId];
                Vector3 srcReferencePoint = symmetryReferencePoints[srcId];
                for (int j = srcId + 1; j < indices.Count; j++)
                {
                    int tgtID = indices[j];
                    var targetHypothesis = symmetries[j];
                    Vector3 targetReferencePoint = symmetryReferencePoints[j];
                    if (MAX_REFERENCE_POINT_DISTANCE <= 0 || PointToPointDistance(srcReferencePoint, targetReferencePoint) < MAX_REFERENCE_POINT_DISTANCE)
                    {
                        var referencePoint = (srcReferencePoint + targetReferencePoint) / 2f;
                        srcHypothesis.ReflectedSymmetryDifference(targetHypothesis, referencePoint, out float angleDiff, out float distanceDiff);
                        if (angleDiff < MAX_NORMAL_ANGLE_DIFF && distanceDiff < MAX_DISTANCE_DIFF)
                        {
                            symmetryAdjacency.AddEdge(i, j);
                        }
                    }
                }
            }
            BronKerbosch bk = new BronKerbosch(symmetryAdjacency);
            List<IList<int>> cliques = bk.RunAlgorithm(2);
            List<IList<int>> hypothesisClusters = new List<List<int>>();
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

                // Remove hyptheses belonging to the largest clique from existing cliques
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
                int bestHypothesisId = -1;
                for (int i = 0; i < cluster.Count; i++)
                {
                    int hypothesisId = indices[cluster[i]];
                    if (occlusionScores[hypothesisId] < minScore)
                    {
                        minScore = occlusionScores[hypothesisId];
                        bestHypothesisId = hypothesisId;
                    }

                    mergedSymmetryIds.Add(bestHypothesisId);
                }
            }
            return mergedSymmetryIds;
        }

        private List<ReflectionalSymmetry> GetInitialReflectionSymmetries(PointCloud cloud)
        {
            List<ReflectionalSymmetry> symmetries = new List<ReflectionalSymmetry>();
            PCA pcaSolver = new PCA(cloud);
            
            float[,] basis = pcaSolver.EigenVectors;

            // Make sure that major axes form a right-handed coordinate system
            if (Vector3.Dot(Vector3.Cross(basis.GetCol(0), basis.GetCol(1)), basis.GetCol(2)) < 0)
            {
                var col2 = basis.GetCol(2);
                basis[0, 2] = col2.X * -1;
                basis[1, 2] = col2.Y * -1;
                basis[2, 2] = col2.Z * -1;
            }

            List<Vector3> spherePoints = GenerateSpherePoints(5);

            foreach (var point in spherePoints)
            {
                symmetries.Add(new ReflectionalSymmetry(pcaSolver.Mean, point.MultiplyVector(basis)));
            }

            return symmetries;
        }

        private List<Vector3> GenerateSpherePoints(int numSegments)
        {
            List<Vector3> spherePoints = new List<Vector3>();
            if (numSegments <= 1)
            {
                throw new Exception("Number of Divisions must be greater than 1");
            }

            var angularStep = MathF.PI / numSegments;
            bool needPolarPoint = numSegments % 2 == 0;
            if (needPolarPoint)
            {
                spherePoints.Add(Vector3.UnitZ);
            }
            List<Vector3> equatorPoints = new List<Vector3>();
            equatorPoints.Add(Vector3.UnitX);
            for (int i = 1; i < numSegments; i++)
            {
                float angle = angularStep * i;
                float[,] angleAxis = CalculateAngleAxis(angle, Vector3.UnitZ);
                equatorPoints.Add(equatorPoints[0].MultiplyVector(angleAxis));
            }
            spherePoints.AddRange(equatorPoints);

            for (int i = 1; i < numSegments; i++)
            {
                if (needPolarPoint && (i * 2 == numSegments))
                    continue;

                List<Vector3> currentNonEquatorPoints = new List<Vector3>();

                for (int j = 0; j < equatorPoints.Count; j++)
                {
                    var currentEquatorPoint = equatorPoints[j];
                    var rotationAxis = Vector3.Cross(currentEquatorPoint, Vector3.UnitZ);
                    float[,] angleAxis = CalculateAngleAxis(angularStep * i, rotationAxis);
                    currentNonEquatorPoints.Add(currentEquatorPoint.MultiplyVector(angleAxis));
                }
                spherePoints.AddRange(currentNonEquatorPoints);
            }
            return spherePoints;
        }

        public static float PointToPointDistance(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        private PointCloud ProjectCloudToPlane(PointCloud original, Vector3 planePoint, Vector3 planeNormal)
        {
            PointCloud projected = original.Clone();
            foreach (var point in projected.Points)
            {
                point.Position = ProjectPointToPlane(point.Position, planePoint, planeNormal);
            }
            return projected;
        }

        public static Vector3 ProjectPointToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return point - planeNormal * PointToPlaneSignedDistance(point, planePoint, planeNormal);
        }

        private bool RefineSymmetryPosition(PointCloud cloud, ReflectionalSymmetry originalSymmetry)
        {
            bool refineCompleted = false;
            ReflectionalSymmetry refined = originalSymmetry;
            List<Correspondence> correspondences = new List<Correspondence>();
            const float minimumSymmetryCorrespondanceDistance = 0.02f;
            float maxSymmetryNormalFitError = (10f).ConvertToRadians();

            var origin = refined.Origin;
            var normal = refined.Normal;

            PointCloud cloudProjected = ProjectCloudToPlane(cloud, origin, normal);

            foreach (var point in cloudProjected.Points)
            {
                var srcPoint = point.Position;
                var srcNormal = point.Normal;
                var neighbours = cloudProjected.GetNeighbours(point);
                float minimumNormalFitError = float.MaxValue;
                PointXYZRGBNormal bestFit = null;

                foreach (var neighbour in neighbours)
                {
                    var targetPos = neighbour.neighbour.Position;
                    var targetNormal = neighbour.neighbour.Normal;

                    var origDistance = originalSymmetry.PointSignedDistance(point.Position);
                    var neighbourDistance = originalSymmetry.PointSignedDistance(neighbour.neighbour.Position);

                    if (MathF.Abs(origDistance - neighbourDistance) >= minimumSymmetryCorrespondanceDistance)
                    {
                        float symNormalFitError = GetReflectionSymmetryNormalFitError(srcNormal, targetNormal, refined);
                        if (symNormalFitError <= maxSymmetryNormalFitError)
                        {
                            if (symNormalFitError < minimumNormalFitError)
                            {
                                minimumNormalFitError = symNormalFitError;
                                bestFit = neighbour.neighbour;
                            }
                        }
                    }
                }
                if (bestFit != null)
                {
                    correspondences.Add(new Correspondence(point, bestFit, minimumNormalFitError));
                }
            }

            if (correspondences.Count > 0)
            {
                refineCompleted = true;

                List<double> positionFitErrors = new List<double>();
                foreach (var correspondance in correspondences)
                {
                    var src = correspondance.Original.Position;
                    var target = correspondance.CorrespondingPoint.Position;
                    positionFitErrors.Add((double)GetReflectionSymmetryPositionFitError(src, target, originalSymmetry));
                }

                float medianPositionFitError = (float)Measures.Median(positionFitErrors.ToArray());
                refined.Origin = origin + normal * medianPositionFitError;
            }

            return refineCompleted;
        }

        public bool RefineGlobalSymmetryPosition(PointCloud cloud, ReflectionalSymmetry symmetry)
        {
            bool success = true;
            ReflectionalSymmetry refinedSymmetry = symmetry;
            ReflectionalSymmetry previousSymmetry;
            List<Correspondence> correspondences;
            int numIterations = 0;

            float MAX_SYMMETRY_NORMAL_FIT_ERROR = (45f).ConvertToRadians();

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

                    Vector3 srcPointReflected = refinedSymmetry.ReflectPoint(srcPoint);
                    Vector3 srcNormalReflected = refinedSymmetry.ReflectNormal(srcNormal);
                    PointXYZRGBNormal searchPoint = new PointXYZRGBNormal()
                    {
                        Position = srcPointReflected,
                        Normal = srcNormalReflected
                    };

                    var neighbours = cloud.GetClosetNeighbours(searchPoint, 1);

                    Vector3 targetPoint = neighbours[0].neighbour.Position;
                    Vector3 targetNormal = neighbours[0].neighbour.Normal;

                    // NOTE: this is required for faster convergence. Distance along symmetry
                    // normal works faster than point to point distnace
                    // If the distance along the symmetry normal between the points of a symmetric correspondence is too small - reject
                    if (MathF.Abs(symmetry.PointSignedDistance(srcPoint) - symmetry.PointSignedDistance(targetPoint)) >= MIN_SYMMETRY_CORRESPONDENCE_DISTANCE)
                    {
                        // If the distance between the reflected source point and it's nearest neighbor is too big - reject
                        if (neighbours[0].distance <= MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE)
                        {
                            // Reject correspondence if normal error is too high
                            float error = GetReflectionSymmetryNormalFitError(srcNormal, targetNormal, refinedSymmetry);
                            if(error <= MAX_SYMMETRY_NORMAL_FIT_ERROR)
                            {
                                correspondences.Add(new Correspondence(point, neighbours[0].neighbour, neighbours[0].distance));
                            }
                        }
                    }

                }
                correspondences = rejector.GetRemainingCorrespondences(correspondences);
                if(correspondences.Count > 0)
                {
                    float[] input = new float[6]
                    {
                        symmetryOrigin.X,
                        symmetryOrigin.Y,
                        symmetryOrigin.Z,
                        symmetryNormal.X,
                        symmetryNormal.Y,
                        symmetryNormal.Z,
                    };
                    functor.Correspondences = correspondences;

                    LevenbergMarquardt lm = new LevenbergMarquardt(functor);
                    lm.Minimise(input);

                    Vector6 solution = new Vector6()
                    {
                        X = input[0],
                        Y = input[1],
                        Z = input[2],
                        A = input[3],
                        B = input[4],
                        C = input[5],
                    };
                    refinedSymmetry = new ReflectionalSymmetry(solution.Head, solution.Tail);
                    refinedSymmetry.SetOriginProjected(cloud.Mean); // seems to be overwriting the above line by setting origin to the 

                    if(++numIterations >= MAX_ITERATIONS)
                    {
                        done = true;
                    }
                    refinedSymmetry.ReflectedSymmetryDifference(previousSymmetry, cloud.Mean, out float angleDiff, out float distanceDiff);
                    
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

        public static float PointToPlaneSignedDistance(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            Vector3 planeToPointVector = point - planePoint;
            return Vector3.Dot(planeToPointVector, planeNormal);
        }

        private void CalculateSymmetryPointSymmetryScores(PointCloud cloud, ReflectionalSymmetry symmetry, out List<float> pointSymmetryScores, out List<Correspondence> correspondences)
        {
            pointSymmetryScores = new List<float>();
            correspondences = new List<Correspondence>();

            float minInlierNormalAngle = (10f).ConvertToRadians();
            float maxInlierNormalAngle = (15f).ConvertToRadians();

            for (int i = 0; i < cloud.Points.Count; i++)
            {
                var point = cloud.Points[i];
                // Get point normal
                Vector3 srcPoint = point.Position;
                Vector3 srcNormal = point.Normal;

                // Reflect point and normal
                Vector3 srcPointReflected = symmetry.ReflectPoint(srcPoint);
                Vector3 srcNormalReflected = symmetry.ReflectNormal(srcNormal);

                // Find neighbours in a radius   
                PointXYZRGBNormal searchPoint = new PointXYZRGBNormal()
                {
                    Position = srcPointReflected,
                    Normal = srcNormalReflected
                };

                var neighbours = cloud.GetClosetNeighbours(searchPoint, 1);
                if(neighbours[0].distance <= MAX_SYMMETRY_CORRESPONDENCE_REFLECTED_DISTANCE)
                {
                    Vector3 targetPoint = neighbours[0].neighbour.Position;
                    Vector3 targetNormal = neighbours[0].neighbour.Normal;

                    // If point belongs to segment boundary, we reduce it's score in half, since normals at the boundary of the segment are usually noisy - noise isn't something we need to worry about so ignore for the moment
                    //if (std::find(cloud_ds_boundary_point_ids.begin(), cloud_ds_boundary_point_ids.end(), pointId) != cloud_ds_boundary_point_ids.end() ||
                    //      std::find(cloud_boundary_point_ids.begin(), cloud_boundary_point_ids.end(), neighbours[0]) != cloud_boundary_point_ids.end())
                    //    continue;

                    float symmetryScore = GetReflectionSymmetryNormalFitError(srcNormal, targetNormal, symmetry);

                    // NOTE: this is to avoid the problem with correspondences between thin walls.
                    // Correspondences between thin walls are likely to have normals that are 180 degrees appart.
                    if (symmetryScore > MathF.PI * 3 / 4)
                        symmetryScore = MathF.PI - symmetryScore;

                    symmetryScore = (symmetryScore - minInlierNormalAngle) / (maxInlierNormalAngle - minInlierNormalAngle);
                    symmetryScore = symmetryScore.ClampValue(0f, 1f);

                    // If all checks passed - add correspondence
                    pointSymmetryScores.Add(symmetryScore);
                    correspondences.Add(new Correspondence(point, neighbours[0].neighbour, neighbours[0].distance));
                }
            }
        }

        private float GetReflectionSymmetryNormalFitError(Vector3 normal1, Vector3 normal2, ReflectionalSymmetry symmetry)
        {
            Vector3 normal2Reflected = symmetry.ReflectNormal(normal2);

            float dotProd = Vector3.Dot(normal1, normal2Reflected).ClampValue(-1, 1);

            return MathF.Acos(dotProd);
        }

        private float GetReflectionSymmetryPositionFitError(Vector3 point1, Vector3 point2, ReflectionalSymmetry symmetry)
        {
            Vector3 midpoint = (point1 + point2) / 2;
            return symmetry.PointSignedDistance(midpoint);
        }

      
        private float[,] CalculateAngleAxis(float angle, Vector3 axis)
        {
            float[,] res = new float[3, 3];
            Vector3 sin_axis = MathF.Sin(angle) * axis;
            float c = MathF.Cos(angle);
            Vector3 cos1_axis = (1 - c) * axis;

            float tmp;
            tmp = cos1_axis.X * axis.Y;
            res[0, 1] = tmp - sin_axis.Z;
            res[1, 0] = tmp + sin_axis.Z;

            tmp = cos1_axis.X * axis.Z;
            res[0, 2] = tmp + sin_axis.Y;
            res[2, 0] = tmp - sin_axis.Y;

            tmp = cos1_axis.Y * axis.Z;
            res[1, 2] = tmp - sin_axis.X;
            res[2, 1] = tmp + sin_axis.X;
            //res.diagonal() = Vector3.Cross(cos1_axis, axis) + c;
            return res;
        }
    }
}

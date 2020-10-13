using SymmetryDetection.Clustering;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.FileTypes;
using SymmetryDetection.FileTypes.PLY;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class SymmetryDetectionHandler
    {
        private PointCloud Cloud { get; set; }
        private PCA ComponentsAnalysis { get; set; }
        private IEnumerable<ISymmetryDetector> SymmetryDetectors { get; set; }
        public List<ISymmetry> Symmetries { get; set; }

        public SymmetryDetectionHandler(IFileType file, IEnumerable<ISymmetryDetector> symmetryDetectors/*, IPrincipalComponentsAnalyser pca, ISymmetryProperties properties*/)
        {
            Cloud = file.ConvertToPointCloud();
            ComponentsAnalysis = new PCA(Cloud);
            SymmetryDetectors = symmetryDetectors;
            Symmetries = new List<ISymmetry>();
        }

        public void DetectSymmetries()
        {
            var cloud = ComponentsAnalysis.DemeanedCloud;
            List<List<int>> segments = new List<List<int>>();
            segments.Add(new List<int>());
            for (int i = 0; i < cloud.Points.Count; i++)
            {
                segments[0].Add(i);
            }
            List<List<int>> supportSegments = new List<List<int>>();

            if (cloud.Points.Count > 3)
            {
                foreach (var detector in SymmetryDetectors)
                {
                    List<PointCloud> segmentClouds = new List<PointCloud>();
                    List<List<ISymmetry>> detectorSymmetries = new List<List<ISymmetry>>();
                    List<List<int>> symmetryFilteredIdsTEMP = new List<List<int>>();
                    List<List<int>> symmetryMergedIdsTEMP = new List<List<int>>();
                    List<List<float>> occlusionScoresTEMP = new List<List<float>>();
                    List<List<float>> correspondenceInlierScoresTEMP = new List<List<float>>();
                    List<List<float>> cloudInlierScoresTEMP = new List<List<float>>();

                    for (int i = 0; i < segments.Count; i++)
                    {
                        segmentClouds.Add(cloud.Clone());
                        //segmentClouds[i].GetCloudBoundary(); //Not sure this is even used
                        detector.SetCloud(segmentClouds[i]);
                        detector.SetPCA(new PCA(segmentClouds[i]));

                        detector.Detect();
                        detector.Filter(); // filter based on score
                        detector.Merge();

                        //Replication of GetSymmetries Method
                        detectorSymmetries.Add(detector.SymmetriesRefined);
                        symmetryFilteredIdsTEMP.Add(detector.SymmetryFilteredIds);
                        symmetryMergedIdsTEMP.Add(detector.SymmetryMergedIds);

                        //Replication of GetScoresMethod
                        occlusionScoresTEMP.Add(detector.OcclusionScores);
                        cloudInlierScoresTEMP.Add(detector.CloudInlierScores);
                        correspondenceInlierScoresTEMP.Add(detector.CorrespondenceInlierScores);
                    }

                    // Linearize symmetry data - similar to SelectMany
                    List<ISymmetry> symmetryLinear = new List<ISymmetry>();
                    List<(int, int)> symmetryLinearMap = new List<(int, int)>();
                    List<float> supportSizesLinear = new List<float>();
                    List<float> occlusionScoresLinear = new List<float>();
                    List<Vector3> referencePointsLinear = new List<Vector3>();
                    for (int i = 0; i < symmetryMergedIdsTEMP.Count; i++)
                    {
                        for (int j = 0; j < symmetryMergedIdsTEMP[i].Count; j++)
                        {
                            int symId = symmetryMergedIdsTEMP[i][j];
                            symmetryLinear.Add(detectorSymmetries[i][symId]);
                            symmetryLinearMap.Add((i, symId));
                            supportSizesLinear.Add(segmentClouds[i].Points.Count);
                            referencePointsLinear.Add(segmentClouds[i].Mean);
                            occlusionScoresLinear.Add(occlusionScoresTEMP[i][symId]);
                        }
                    }

                    List<int> indices = new List<int>();
                    for(int i = 0; i < symmetryLinear.Count; i++)
                    {
                        indices.Add(i);
                    }
                    //merge symmetries for the entire cloud
                    List<int> symmetryMergedGlobalIdsLinear = detector.MergeDuplicateSymmetries(symmetryLinear, indices, referencePointsLinear, detector.OcclusionScores);

                    //construct Final output
                    foreach (var merged in symmetryMergedGlobalIdsLinear)
                    {
                        int symLinId = merged;
                        int segId = symmetryLinearMap[merged].Item1;
                        int symId = symmetryLinearMap[merged].Item2;

                        Symmetries.Add(detectorSymmetries[segId][symId]);
                        supportSegments.Add(segments[segId]);
                    }
                }
            }
            DeMeanSymmetries();
        }

        public float CalculateGlobalSymmetryScore()
        {
            float scoreSum = 0;
            foreach (var detector in SymmetryDetectors)
            {
                var typeScore = detector.CalculateGlobalScore();
                scoreSum += typeScore * ((float)detector.SymmetryMergedIds.Count / 29);
            }
            return scoreSum / SymmetryDetectors.Count();
        }

        private void DeMeanSymmetries()
        {
            foreach (var symmetry in Symmetries)
            {
                symmetry.Origin = symmetry.Origin + ComponentsAnalysis.Mean;
            }
        }
    }
}   


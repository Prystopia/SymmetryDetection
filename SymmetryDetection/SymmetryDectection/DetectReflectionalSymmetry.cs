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
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    public class DetectReflectionalSymmetry
    {
        private PointCloud Cloud { get; set; }
        private PCA ComponentsAnalysis { get; set; }
        public List<ReflectionalSymmetry> Symmetries { get; set; }

        public DetectReflectionalSymmetry(IFileType file)
        {
            Cloud = file.ConvertToPointCloud();
            ComponentsAnalysis = new PCA(Cloud);

        }

        public string GetExportFile()
        {
            return Symmetries.GetExportFile();
        }

        public void DetectReflectionalSymmetries()
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
                List<PointCloud> segmentClouds = new List<PointCloud>();
                List<List<ReflectionalSymmetry>> symmetryTEMP = new List<List<ReflectionalSymmetry>>();
                List<List<int>> symmetryFilteredIdsTEMP = new List<List<int>>();
                List<List<int>> symmetryMergedIdsTEMP = new List<List<int>>();
                List<List<float>> occlusionScoresTEMP = new List<List<float>>();
                List<List<float>> correspondenceInlierScoresTEMP = new List<List<float>>();
                List<List<float>> cloudInlierScoresTEMP = new List<List<float>>();

                for (int i = 0; i < segments.Count; i++)
                {
                    segmentClouds.Add(cloud.Clone());
                    //segmentClouds[i].GetCloudBoundary(); //Not sure this is even used
                    ReflectionalSymmetryDetection rsd = new ReflectionalSymmetryDetection(segmentClouds[i], new PCA(segmentClouds[i]));
                    rsd.Detect();
                    rsd.Filter();
                    rsd.Merge();

                    //Replication of GetSymmetries Method
                    symmetryTEMP.Add(rsd.SymmetriesRefined);
                    symmetryFilteredIdsTEMP.Add(rsd.SymmetryFilteredIds);
                    symmetryMergedIdsTEMP.Add(rsd.SymmetryMergedIds);

                    //Replication of GetScoresMethod
                    occlusionScoresTEMP.Add(rsd.OcclusionScores);
                    cloudInlierScoresTEMP.Add(rsd.CloudInlierScores);
                    correspondenceInlierScoresTEMP.Add(rsd.CorrespondenceInlierScores);
                }

                // Linearize symmetry data - similar to SelectMany
                List<ReflectionalSymmetry> symmetryLinear = new List<ReflectionalSymmetry>();
                List<(int, int)> symmetryLinearMap = new List<(int, int)>();
                List<float> supportSizesLinear = new List<float>();
                List<float> occlusionScoresLinear = new List<float>();
                List<Vector3> referencePointsLinear = new List<Vector3>();
                for (int i = 0; i < symmetryMergedIdsTEMP.Count; i++)
                {
                    for (int j = 0; j < symmetryMergedIdsTEMP[i].Count; j++)
                    {
                        int symId = symmetryMergedIdsTEMP[i][j];
                        symmetryLinear.Add(symmetryTEMP[i][symId]);
                        symmetryLinearMap.Add((i, symId));
                        supportSizesLinear.Add(segmentClouds[i].Points.Count);
                        referencePointsLinear.Add(segmentClouds[i].Mean);
                        occlusionScoresLinear.Add(occlusionScoresTEMP[i][symId]);
                    }
                }
                ReflectionalSymmetryDetection rsdGlobal = new ReflectionalSymmetryDetection(cloud, new PCA(cloud));
                //merge symmetries for the entire cloud
                List<int> symmetryMergedGlobalIdsLinear = rsdGlobal.MergeDuplicateReflectedSymmetries(symmetryLinear, referencePointsLinear,occlusionScoresLinear);

                //construct Final output
                foreach (var merged in symmetryMergedGlobalIdsLinear)
                {
                    int symLinId = merged;
                    int segId = symmetryLinearMap[merged].Item1;
                    int symId = symmetryLinearMap[merged].Item2;

                    Symmetries.Add(symmetryTEMP[segId][symId]);
                    supportSegments.Add(segments[segId]);
                }
            }
            UnMeanCloud();
        }

        private void UnMeanCloud()
        {
            foreach (var symmetry in Symmetries)
            {
                symmetry.Origin = symmetry.Origin + ComponentsAnalysis.Mean;
            }
        }
    }
}   


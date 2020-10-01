using Accord.Statistics;
using SymmetryDetection.Clustering;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.FileTypes;
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

        public void Main()
        {
            var ply = LoadPlyFile("");
            var pc = InitialisePointCloud(ply);
            var pca = new PCA(pc);
            //only required if we are downscaling - NOT IMPLEMENTED CURRENTLY
            //GetMinMax(pc, out Vector3 min, out Vector3 max);
            var demeaned = pca.DemeanedCloud;
            var symmetries = DetectReflectionalSymmetryScene(demeaned);
            Console.WriteLine($"{symmetries.Count} reflectional symmetries detected");

            //unmean cloud
            foreach (var symmetry in symmetries)
            {
                symmetry.Origin = symmetry.Origin + pca.Mean;
            }
            //Write symmetries to file
            string exportFile = symmetries.GetExportFile();
            File.WriteAllText(@$"C:\Temp\symmetries.{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-mi")}.csv", exportFile);
        }
        private PLYFile LoadPlyFile(string fileLoc)
        {
            PLYFile file = new PLYFile();
            file.ReadFile(fileLoc);
            return file;
        }
        private PointCloud InitialisePointCloud(PLYFile file)
        {
            PointCloud cloud = new PointCloud();
            foreach (var vert in file.Vertices)
            {
                cloud.Points.Add(new PointXYZRGBNormal()
                {
                    Colour = vert.Colour,
                    Position = vert.Position,
                    Curvature = vert.Curvature,
                    Normal = vert.Normal
                });
            }
            return cloud;
        }
        private void GetMinMax(PointCloud cloud, out Vector3 min, out Vector3 max)
        {
            min = new Vector3(float.MaxValue);
            max = new Vector3(float.MinValue);
            foreach (var point in cloud.Points)
            {
                min = Vector3.Min(min, point.Position);
                max = Vector3.Max(max, point.Position);
            }
        }
        private List<ReflectionalSymmetry> DetectReflectionalSymmetryScene(PointCloud cloud)
        {
            List<ReflectionalSymmetry> symmetries = new List<ReflectionalSymmetry>();
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
                    segmentClouds[i] = cloud.Clone();
                    segmentClouds[i].GetCloudBoundary(); //Not sure this is even used
                    ReflectionalSymmetryDetection rsd = new ReflectionalSymmetryDetection(segmentClouds[i]);
                    rsd.Detect();
                    rsd.Filter();
                    rsd.Merge();

                    //Replication of GetSymmetries Method
                    symmetryTEMP[i] = rsd.SymmetriesRefined;
                    symmetryFilteredIdsTEMP[i] = rsd.SymmetryFilteredIds;
                    symmetryMergedIdsTEMP[i] = rsd.SymmetryMergedIds;

                    //Replication of GetScoresMethod
                    occlusionScoresTEMP[i] = rsd.OcclusionScores;
                    cloudInlierScoresTEMP[i] = rsd.CloudInlierScores;
                    correspondenceInlierScoresTEMP[i] = rsd.CorrespondenceInlierScores;
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
                    }
                }
                ReflectionalSymmetryDetection rsdGlobal = new ReflectionalSymmetryDetection(cloud);
                //merge symmetries for the entire cloud
                List<int> symmetryMergedGlobalIdsLinear = rsdGlobal.MergeDuplicateReflectedSymmetries(symmetryLinear, referencePointsLinear,occlusionScoresLinear);

                //construct Final output
                foreach (var merged in symmetryMergedGlobalIdsLinear)
                {
                    int symLinId = merged;
                    int segId = symmetryLinearMap[merged].Item1;
                    int symId = symmetryLinearMap[merged].Item2;

                    symmetries.Add(symmetryTEMP[segId][symId]);
                    supportSegments.Add(segments[segId]);
                }
            }
            return symmetries;
        }
    }
}   


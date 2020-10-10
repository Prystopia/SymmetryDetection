using SymmetryDetection.Clustering;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.FileTypes;
using SymmetryDetection.FileTypes.PLY;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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

        public SymmetryDetectionHandler(IFileType file, IEnumerable<ISymmetryDetector> symmetryDetectors/*, IPrincipalComponentsAnalyser pca*/)
        {
            Cloud = file.ConvertToPointCloud();
            ComponentsAnalysis = new PCA(Cloud);
            SymmetryDetectors = symmetryDetectors;
            Symmetries = new List<ISymmetry>();
        }

        public void DetectSymmetries()
        {
            var cloud = ComponentsAnalysis.DemeanedCloud;
            if (cloud.Points.Count > 3)
            {
                foreach (var detector in SymmetryDetectors)
                {
                    detector.SetCloud(cloud);
                    detector.SetPCA(new PCA(cloud));

                    detector.Detect();
                    detector.Filter(); // filter based on score
                    detector.Merge();

                    Symmetries.AddRange(detector.MergedSymmetries);
                }
            }
            DeMeanSymmetries();
        }
        public float CalculateGlobalSymmetryScore()
        {
            float scoreSum = 0;
            foreach(var detector in SymmetryDetectors)
            {
                var typeScore = detector.CalculateGlobalSymmetryScore();
                scoreSum += typeScore * ((float)detector.MergedSymmetries.Count / detector.MaxPlanes);
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


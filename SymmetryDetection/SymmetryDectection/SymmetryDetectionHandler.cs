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

        public SymmetryDetectionHandler(IFileType file, IEnumerable<ISymmetryDetector> symmetryDetectors)
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
                    List<ISymmetry> detectorSymmetries = new List<ISymmetry>();

                    detector.SetCloud(cloud.Clone());
                    PCA analysis = new PCA(cloud);
                    detector.SetPCA(analysis);

                    detector.Detect();
                    detector.Filter();
                    detector.Merge();

                    Symmetries.AddRange(detector.MergedSymmetries);
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
                scoreSum += typeScore * ((float)detector.MergedSymmetries.Count / 29);
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


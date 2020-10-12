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
        private ISymmetryDetector<ReflectionalSymmetry> ReflectionalSymmetryDetector { get; set; }
        private ISymmetryDetector<RotationalSymmetry> RotationalSymmetryDetector { get; set; }
        public List<ISymmetry> Symmetries { get; set; }

        public SymmetryDetectionHandler(IFileType file, ISymmetryDetector<ReflectionalSymmetry> reflectionalDetector, ISymmetryDetector<RotationalSymmetry> rotationalDetector)
        {
            Cloud = file.ConvertToPointCloud();
            ComponentsAnalysis = new PCA(Cloud);
            ReflectionalSymmetryDetector = reflectionalDetector;
            RotationalSymmetryDetector = rotationalDetector;
            Symmetries = new List<ISymmetry>();
        }

        public void DetectSymmetries()
        {
            var cloud = ComponentsAnalysis.DemeanedCloud;
            if (cloud.Points.Count > 3)
            {
                if (ReflectionalSymmetryDetector != null)
                {
                    ReflectionalSymmetryDetector.SetCloud(cloud);
                    ReflectionalSymmetryDetector.SetPCA(new PCA(cloud));

                    ReflectionalSymmetryDetector.Detect();
                    ReflectionalSymmetryDetector.Filter(); // filter based on score
                    ReflectionalSymmetryDetector.Merge();
                    Symmetries.AddRange(ReflectionalSymmetryDetector.MergedSymmetries);
                }
                if (RotationalSymmetryDetector != null)
                {
                    RotationalSymmetryDetector.SetCloud(cloud);
                    RotationalSymmetryDetector.SetPCA(new PCA(cloud));

                    RotationalSymmetryDetector.Detect();
                    RotationalSymmetryDetector.Filter(); // filter based on score
                    RotationalSymmetryDetector.Merge();
                    Symmetries.AddRange(RotationalSymmetryDetector.MergedSymmetries);
                }
            }
            DeMeanSymmetries();
        }
        public float CalculateGlobalSymmetryScore()
        {
            float scoreSum = 0;
            float detectorCount = 0;
            if(ReflectionalSymmetryDetector != null)
            {
                detectorCount++;
                var typeScore = ReflectionalSymmetryDetector.CalculateGlobalSymmetryScore();
                scoreSum += typeScore * ((float)ReflectionalSymmetryDetector.MergedSymmetries.Count / ReflectionalSymmetryDetector.MaxPlanes);
            }

            if (RotationalSymmetryDetector != null)
            {
                detectorCount++;
                var typeScore = RotationalSymmetryDetector.CalculateGlobalSymmetryScore();
                scoreSum += typeScore * ((float)RotationalSymmetryDetector.MergedSymmetries.Count / RotationalSymmetryDetector.MaxPlanes);
            }

            return scoreSum / detectorCount;
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


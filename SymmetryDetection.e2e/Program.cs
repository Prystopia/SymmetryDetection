using Newtonsoft.Json;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Exporters;
using SymmetryDetection.Extensions;
using SymmetryDetection.FileTypes;
using SymmetryDetection.FileTypes.PLY;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.Parameters;
using SymmetryDetection.ScoreServices;
using SymmetryDetection.SymmetryDectection;
using SymmetryDetection.SymmetryDetectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SymmetryDetection.e2e
{
    class Program
    {
        private static void TestUnitCube()
        {
            Console.WriteLine("Detecting symmetries for unit cube");

            TestFileType testFile = new TestFileType(new List<Vector3>()
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f , 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
            });

            //expected symmetry score
            testFile.SymmetryScore = 0.692f;

            List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
            {
                new ReflectionalSymmetryDetector(new DistanceErrorScoreService()),
            };
            ISymmetryExporter exporter = new TextExporter();
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(testFile, handlers);
            drs.DetectSymmetries();

            if (drs.Symmetries.Count(s=>s.SymmetryType == Enums.SymmetryTypeEnum.Reflectional) != 9)
            {
                throw new Exception("Incorrect number of symmetry planes detected");
            }
            if (!drs.Symmetries.All(s => new Vector3(MathF.Round(s.Origin.X), MathF.Round(s.Origin.Y), MathF.Round(s.Origin.Z)) == new Vector3(0, 0, 0)))
            {
                throw new Exception("Incorrect Origin set for symmetry planes");
            }
            //round to remove any floating point errors
            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(1, 0, 0)))
            {
                throw new Exception("Missing plane (1, 0, 0) in detected symmetries");
            }
            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(0, 1, 0)))
            {
                throw new Exception("Missing plane (0, 1, 0) in detected symmetries");
            }

            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(0, 0, 1)))
            {
                throw new Exception("Missing plane (0, 0, 1) in detected symmetries");
            }

            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(1, 1, 0)))
            {
                throw new Exception("Missing plane (1, 1, 0) in detected symmetries");
            }
            //check for both possible permutations of this plane
            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(1, -1, 0)) && !CheckCollectionContainsVector(drs.Symmetries, new Vector3(-1, 1, 0)))
            {
                throw new Exception("Missing plane (1, -1, 0) in detected symmetries");
            }

            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(0, 1, 1)))
            {
                throw new Exception("Missing plane (1, 1, 0) in detected symmetries");
            }
            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(0, 1, -1)) && !CheckCollectionContainsVector(drs.Symmetries, new Vector3(0, -1, 1)))
            {
                throw new Exception("Missing plane (0, 1, -1) in detected symmetries");
            }

            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(1, 0, 1)))
            {
                throw new Exception("Missing plane (1, 0, 1) in detected symmetries");
            }
            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(1, 0, -1)) && !CheckCollectionContainsVector(drs.Symmetries, new Vector3(-1, 0, 1)))
            {
                throw new Exception("Missing plane (1, 0, -1) in detected symmetries");
            }

            AverageGlobalScoreService averageScoreService = new AverageGlobalScoreService();
            var score = averageScoreService.CalculateGlobalScore(drs);
            if (testFile.SymmetryScore != MathF.Round(score, 3))
            {
                throw new Exception("Unexpected score calculated");
            }

            BestPlaneGlobalScoreService bestPlaneScoreService = new BestPlaneGlobalScoreService();
            var bestPlaneScore = bestPlaneScoreService.CalculateGlobalScore(drs);

            if (1 != MathF.Round(bestPlaneScore, 3))
            {
                throw new Exception("Unexpected best plane score calculated");
            }

            //write output to console
            string export = exporter.ExportSymmetries(drs.Symmetries, score);
            Console.Write(export);
        }

        private static void TestUnitSphere()
        {
            Console.WriteLine("Detecting symmetries for unit sphere");

            List<Vector3> spherePoints = new List<Vector3>();

            //angle along x-axis - radians
            float azimuthalAngleStepRad = 12f.ConvertToRadians();

            //add the polar point
            spherePoints.Add(Vector3.UnitZ);

            //angle along z-axis - radians
            float polarAngleStepRad = 12f.ConvertToRadians();
            float xTotal = (2 * MathF.PI) / azimuthalAngleStepRad;
            float zTotal = MathF.PI / polarAngleStepRad;

            for (float i = 0; i < xTotal; i++)
            {
                //start at 1 to skip the polar point duplication
                for (float j = 1; j < zTotal; j++)
                {
                    Vector3 point = new Vector3();
                    point.X = MathF.Sin(polarAngleStepRad * j) * MathF.Cos(azimuthalAngleStepRad * i);
                    point.Y = MathF.Sin(polarAngleStepRad * j) * MathF.Sin(azimuthalAngleStepRad * i);
                    point.Z = MathF.Cos(polarAngleStepRad * j);
                    spherePoints.Add(point);
                }
            }
           
            TestFileType testFile = new TestFileType(spherePoints);
            testFile.SymmetryScore = 1;

            List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
            {
                new ReflectionalSymmetryDetector(new DistanceErrorScoreService()),
            };
            ISymmetryExporter exporter = new TextExporter();
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(testFile, handlers);
            drs.DetectSymmetries();

            //this is the maximum number of planes we can detect
            if (drs.Symmetries.Count != 13)
            {
                throw new Exception("Incorrect number of symmetry planes detected");
            }
            if (!drs.Symmetries.All(s => MathF.Round(s.Origin.X) == 0 && MathF.Round(s.Origin.Y) == 0 && MathF.Round(s.Origin.Z) == 0))
            {
                throw new Exception("Incorrect Origin set for symmetry planes");
            }

            AverageGlobalScoreService averageScoreService = new AverageGlobalScoreService();
            var score = averageScoreService.CalculateGlobalScore(drs);

            if (testFile.SymmetryScore != MathF.Round(score, 3))
            {
                throw new Exception("Unexpected score calculated");
            }

            BestPlaneGlobalScoreService bestPlaneScoreService = new BestPlaneGlobalScoreService();
            var bestPlaneScore = bestPlaneScoreService.CalculateGlobalScore(drs);

            if (1 != MathF.Round(bestPlaneScore, 3))
            {
                throw new Exception("Unexpected best plane score calculated");
            }

            //write output to console
            string export = exporter.ExportSymmetries(drs.Symmetries, score);
            Console.Write(export);
        }

        private static void AddSymmetryLevelsToSculptures()
        {
            TestUnitCube();
            //TestUnitSphere();
            //TestPerformance();
            
            //could also work here with 2D items, instead of creating sphere points we work with circles to find the possible planes
            //AddSymmetryLevelsToSculptures();
        }

        private static bool CheckCollectionContainsVector(List<ISymmetry> symmetries, Vector3 vector)
        {
            return symmetries.Any(s => Math.Round(s.Normal.X) == vector.X && Math.Round(s.Normal.Y) == vector.Y && Math.Round(s.Normal.Z) == vector.Z);
        }
    }
}

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

            TestFileType testFile = new TestFileType(new List<(Vector3, Vector3)>()
            {
                (new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, 0)),
                (new Vector3(-0.5f, -0.5f , 0.5f), new Vector3(5, 5, 5)),
                (new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(10, 10, 10)),
                (new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(15, 15, 15)),
                (new Vector3(0.5f, -0.5f, -0.5f), new Vector3(20, 20, 20)),
                (new Vector3(0.5f, -0.5f, 0.5f), new Vector3(25, 25, 25)),
                (new Vector3(0.5f, 0.5f, 0.5f), new Vector3(30, 30, 30)),
                (new Vector3(0.5f, 0.5f, -0.5f), new Vector3(35, 35, 35)),
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

        private static void TestColourUnitCubeScore()
        {
            Console.WriteLine("Detecting symmetries for unit cube");

            TestFileType testFile = new TestFileType(new List<(Vector3, Vector3)>()
            {
                (new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, 0)),
                (new Vector3(-0.5f, -0.5f , 0.5f), new Vector3(5, 5, 5)),
                (new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(10, 10, 10)),
                (new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(15, 15, 15)),
                (new Vector3(0.5f, -0.5f, -0.5f), new Vector3(20, 20, 20)),
                (new Vector3(0.5f, -0.5f, 0.5f), new Vector3(25, 25, 25)),
                (new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 0)),
                (new Vector3(0.5f, 0.5f, -0.5f), new Vector3(35, 35, 35)),
            });

            //expected symmetry score
            testFile.SymmetryScore = 0.434f;

            List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
            {
                //need to set these minimum scores to 0 to ensure we capture all planes
                new ReflectionalSymmetryDetector(new ColourErrorScoreService(), new ReflectionalSymmetryParameters(){ MIN_CLOUD_INLIER_SCORE = 0, MIN_CORRESPONDANCE_INLIER_SCORE = 0 }),
            };
            ISymmetryExporter exporter = new TextExporter();
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(testFile, handlers);
            drs.DetectSymmetries();

            //All 9 planes should be detected however the score should be different 

            if (drs.Symmetries.Count(s => s.SymmetryType == Enums.SymmetryTypeEnum.Reflectional) != 9)
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

            if (0.951f != MathF.Round(bestPlaneScore, 3))
            {
                throw new Exception("Unexpected best plane score calculated");
            }

            //write output to console
            string export = exporter.ExportSymmetries(drs.Symmetries, score);
            Console.Write(export);
        }

        private static void TestColourUnitCubeSinglePlaneDetected()
        {
            Console.WriteLine("Detecting symmetries for unit cube");

            TestFileType testFile = new TestFileType(new List<(Vector3, Vector3)>()
            {
                (new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, 0)),
                (new Vector3(-0.5f, -0.5f , 0.5f), new Vector3(5, 5, 5)),
                (new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0, 0, 0)),
                (new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(15, 15, 15)),
                (new Vector3(0.5f, -0.5f, -0.5f), new Vector3(20, 20, 20)),
                (new Vector3(0.5f, -0.5f, 0.5f), new Vector3(25, 25, 25)),
                (new Vector3(0.5f, 0.5f, 0.5f), new Vector3(20, 20, 20)),
                (new Vector3(0.5f, 0.5f, -0.5f), new Vector3(35, 35, 35)),
            });

            //expected symmetry score
            testFile.SymmetryScore = 0.077f;

            List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
            {
                new ReflectionalSymmetryDetector(new DistanceErrorScoreService(), new ReflectionalSymmetryParameters(){ MAX_COLOUR_INTENSITY_DIFF = 0 }),
            };
            ISymmetryExporter exporter = new TextExporter();
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(testFile, handlers);
            drs.DetectSymmetries();

            if (drs.Symmetries.Count(s => s.SymmetryType == Enums.SymmetryTypeEnum.Reflectional) != 1)
            {
                throw new Exception("Incorrect number of symmetry planes detected");
            }
            if (!drs.Symmetries.All(s => new Vector3(MathF.Round(s.Origin.X), MathF.Round(s.Origin.Y), MathF.Round(s.Origin.Z)) == new Vector3(0, 0, 0)))
            {
                throw new Exception("Incorrect Origin set for symmetry planes");
            }
            //round to remove any floating point errors
            if (!CheckCollectionContainsVector(drs.Symmetries, new Vector3(0, 1, 1)))
            {
                throw new Exception("Missing plane (0, 0, 1) in detected symmetries");
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

            List<(Vector3, Vector3)> spherePoints = new List<(Vector3, Vector3)>();

            //angle along x-axis - radians
            float azimuthalAngleStepRad = 12f.ConvertToRadians();

            //add the polar point
            spherePoints.Add((Vector3.UnitZ, new Vector3(102,102,102)));

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
                    var colour = new Vector3(102, 102, 102);
                    spherePoints.Add((point, colour));
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

        static void Main()
        {
            TestUnitCube();
            TestColourUnitCubeSinglePlaneDetected();
            TestColourUnitCubeScore();
            TestUnitSphere();
            //TestPerformance();
        }

        private static bool CheckCollectionContainsVector(List<ISymmetry> symmetries, Vector3 vector)
        {
            return symmetries.Any(s => Math.Round(s.Normal.X) == vector.X && Math.Round(s.Normal.Y) == vector.Y && Math.Round(s.Normal.Z) == vector.Z);
        }
    }
}

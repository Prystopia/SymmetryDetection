using Newtonsoft.Json;
using SymmetryDetection.Exporters;
using SymmetryDetection.Extensions;
using SymmetryDetection.FileTypes;
using SymmetryDetection.FileTypes.PLY;
using SymmetryDetection.Interfaces;
using SymmetryDetection.Parameters;
using SymmetryDetection.ScoreServices;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
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
            testFile.SymmetryScore = 0.310f;

            List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
            {
                new ReflectionalSymmetryDetector(new ReflectionalSymmetryScoreService()),
            };
            ISymmetryExporter exporter = new TextExporter();
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(testFile, handlers);
            drs.DetectSymmetries();

            if (drs.Symmetries.Count(s=>s.SymmetryType == Enums.SymmetryTypeEnum.Reflectional) != 9)
            {
                throw new Exception("Incorrect number of symmetry planes detected");
            }
            if (!drs.Symmetries.All(s => s.Origin == new Vector3(0, 0, 0)))
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

            float score = drs.CalculateGlobalSymmetryScore();
            if (testFile.SymmetryScore != MathF.Round(score, 3))
            {
                throw new Exception("Unexpected score calculated");
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
                new ReflectionalSymmetryDetector(new ReflectionalSymmetryScoreService()),
            };
            ISymmetryExporter exporter = new TextExporter();
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(testFile, handlers);
            drs.DetectSymmetries();

            if (drs.Symmetries.Count != 29)
            {
                throw new Exception("Incorrect number of symmetry planes detected");
            }
            if (!drs.Symmetries.All(s => MathF.Round(s.Origin.X) == 0 && MathF.Round(s.Origin.Y) == 0 && MathF.Round(s.Origin.Z) == 0))
            {
                throw new Exception("Incorrect Origin set for symmetry planes");
            }

            float score = drs.CalculateGlobalSymmetryScore();
            if (testFile.SymmetryScore != MathF.Round(score, 3))
            {
                throw new Exception("Unexpected score calculated");
            }

            //write output to console
            string export = exporter.ExportSymmetries(drs.Symmetries, score);
            Console.Write(export);
        }

        private static void AddSymmetryLevelsToSculptures()
        {
            int totalSculptures = 149, startSculpture = 1;

            for (int i = startSculpture; i < totalSculptures + startSculpture; i++)
            {
                Console.WriteLine($"Detecting symmetries for sculpture: {i}");

                string fileLoc = @$"G:\Git\SculptureGallery\SculptureGallery\SculptureGallery\SculptureGallery\json\axial{i}-reduced.json";
                InstallationDefinitionFile file = JsonConvert.DeserializeObject<InstallationDefinitionFile>(File.ReadAllText(fileLoc));

                ReflectionalSymmetryParameters parameters = new ReflectionalSymmetryParameters();
                parameters.MAX_REFERENCE_POINT_DISTANCE = 5;
                parameters.MIN_CLOUD_INLIER_SCORE = 0.15f;


                List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
                {
                    new ReflectionalSymmetryDetector(new ReflectionalSymmetryScoreService(), parameters),
                    //new RotationalSymmetryDectector()
                };
                ISymmetryExporter exporter = new TextExporter();
                SymmetryDetectionHandler drs = new SymmetryDetectionHandler(file, handlers);
                drs.DetectSymmetries();
                var globalScore = drs.CalculateGlobalSymmetryScore();
                
                file.SymmetryLevel = globalScore;
                File.WriteAllText(fileLoc, JsonConvert.SerializeObject(file, Formatting.Indented));
                string export = exporter.ExportSymmetries(drs.Symmetries, globalScore);

                Console.Write(export);
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            //TestUnitCube();
            //TestUnitSphere();
            AddSymmetryLevelsToSculptures();
        }

        private static bool CheckCollectionContainsVector(List<ISymmetry> symmetries, Vector3 vector)
        {
            return symmetries.Any(s => MathF.Round(s.Normal.X) == vector.X && MathF.Round(s.Normal.Y) == vector.Y && MathF.Round(s.Normal.Z) == vector.Z);
        }
    }
}

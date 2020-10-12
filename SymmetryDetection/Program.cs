using Newtonsoft.Json;
using SymmetryDetection.Exporters;
using SymmetryDetection.FileTypes;
using SymmetryDetection.FileTypes.jsonData;
using SymmetryDetection.FileTypes.PLY;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using SymmetryDetection.SymmetryDetectors;
using SymmetryDetection.SymmetryScorers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SymmetryDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            //ReflectionalSymmetry sym = new ReflectionalSymmetry(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
            int totalSculptures = 2, startSculpture = 1;

            //PLYFile file = new PLYFile();
            //file.LoadFromFile($@"/Users/eddie/untitled.ply");
            //int i = 0;
            for (int i = startSculpture; i < totalSculptures + startSculpture; i++)
            {
                string fileLoc = @$"G:\Git\SculptureGallery\SculptureGallery\SculptureGallery\SculptureGallery\json\axial{i}.json";
                InstallationDefinitionFile file = JsonConvert.DeserializeObject<InstallationDefinitionFile>(File.ReadAllText(fileLoc));
                //var test = file.Items.Count;
                //file.LoadFromFile();
                file.Items = new List<ShapePosition>()
                {
                    new ShapePosition(){ X = 1, Y = 0, Z = 0 },
                     new ShapePosition(){ X = 0, Y = 1, Z = 0 },
                     new ShapePosition(){ X = 0, Y = 0, Z = 1 },
                     new ShapePosition(){ X = 1, Y = 1, Z = 0 },
                     new ShapePosition(){ X = 1, Y = -1, Z = 0 },
                     new ShapePosition(){ X = 0, Y = 1, Z = 1 },
                     new ShapePosition(){ X = 0, Y = 1, Z = -1 },
                     new ShapePosition(){ X = 1, Y = 0, Z = 1 },
                     new ShapePosition(){ X = 1, Y = 0, Z = -1 },
                };
                var reflectionalDetector = new ReflectionalSymmetryDetector(new ReflectionalSymmetryScoreService());
                //(ISymmetryDetector <ISymmetry>)new RotationalSymmetryDetector(new RotationalSymmetryScoreService(), new PerpendicularScoreService(), new CloudCoverageScoreService())
                //ISymmetryExporter exporter = new TextExporter();
                SymmetryDetectionHandler drs = new SymmetryDetectionHandler(file, reflectionalDetector, null);
                drs.DetectSymmetries();
                var globalScore = drs.CalculateGlobalSymmetryScore();
                if(file.SymmetryLevel == globalScore)
                {

                }
                //file.SymmetryLevel = globalScore;
                //File.WriteAllText(fileLoc, JsonConvert.SerializeObject(file, Formatting.Indented));
                //string export = exporter.ExportSymmetries(drs.Symmetries, globalScore);

                //Console.Write(export);
                Console.WriteLine();
            }
        }
    }

}

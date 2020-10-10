using Newtonsoft.Json;
using SymmetryDetection.Exporters;
using SymmetryDetection.FileTypes;
using SymmetryDetection.FileTypes.jsonData;
using SymmetryDetection.FileTypes.PLY;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
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
            int totalSculptures = 149, startSculpture = 1;

            //PLYFile file = new PLYFile();
            //file.LoadFromFile($@"/Users/eddie/untitled.ply");
            //int i = 0;
            for (int i = startSculpture; i < totalSculptures + startSculpture; i++)
            {
                string fileLoc = @$"G:\Git\SculptureGallery\SculptureGallery\SculptureGallery\SculptureGallery\json\axial{i}.json";
                InstallationDefinitionFile file = JsonConvert.DeserializeObject<InstallationDefinitionFile>(File.ReadAllText(fileLoc));
                //file.LoadFromFile();
                //file.Items = new List<ShapePosition>()
                //{
                //    new ShapePosition(){ X = 0.5f, Y = 0, Z = 0 },
                //     new ShapePosition(){ X = 0, Y = 1, Z = 0 },
                //     new ShapePosition(){ X = 0, Y = 0, Z = 1 },
                //     new ShapePosition(){ X = 1, Y = 1, Z = 0 },
                //     new ShapePosition(){ X = 1, Y = -1, Z = 0 },
                //     new ShapePosition(){ X = 0, Y = 1, Z = 1 },
                //     new ShapePosition(){ X = 0, Y = 1, Z = -1 },
                //     new ShapePosition(){ X = 1, Y = 0, Z = 1 },
                //     new ShapePosition(){ X = 1, Y = 0, Z = -1 },
                //};

                List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
                {
                    new ReflectionalSymmetryDetector(new ReflectionalSymmetryScoreService()),
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
    }

}

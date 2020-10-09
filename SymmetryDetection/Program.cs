using SymmetryDetection.Exporters;
using SymmetryDetection.FileTypes;
using SymmetryDetection.FileTypes.PLY;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SymmetryDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            ReflectionalSymmetry sym = new ReflectionalSymmetry(new Vector3(0, 0, 0), new Vector3(1, 0, 0));


            //PLYFile file = new PLYFile();
            //file.LoadFromFile($@"/Users/eddie/untitled.ply");

            InstallationDefinitionFile file = new InstallationDefinitionFile();
            file.LoadFromFile("/Users/eddie/SculptureGallery/SculptureGallery/SculptureGallery/SculptureGallery/json/axial1.json");
            List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
            {
                new ReflectionalSymmetryDetector(),
                //new RotationalSymmetryDectector()
            };
            ISymmetryExporter exporter = new TextExporter();
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(file, handlers);
            drs.DetectSymmetries();
            string export = exporter.ExportSymmetries(drs.Symmetries);
            Console.WriteLine($"{drs.Symmetries.Count} reflectional symmetries detected");
            Console.Write(export);
            //write to file
        }
    }

}

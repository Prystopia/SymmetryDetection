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
            PLYFile file = new PLYFile();
            file.LoadFromFile($@"/Users/Eddie/Untitled.ply");
            List<ISymmetryDetector> handlers = new List<ISymmetryDetector>()
            {
                new ReflectionalSymmetryDetector()
            };
            SymmetryDetectionHandler drs = new SymmetryDetectionHandler(file, handlers);
            drs.DetectReflectionalSymmetries();
            string export = drs.GetExportFile();
            Console.WriteLine($"{drs.Symmetries.Count} reflectional symmetries detected");
            //write to file
        }
    }

}

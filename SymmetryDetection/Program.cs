using SymmetryDetection.FileTypes.PLY;
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
            DetectReflectionalSymmetry drs = new DetectReflectionalSymmetry(file);
            drs.DetectReflectionalSymmetries();
            string export = drs.GetExportFile();
            Console.WriteLine($"{drs.Symmetries.Count} reflectional symmetries detected");
            //write to file
        }
    }

}

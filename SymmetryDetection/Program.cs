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
            DetectReflectionalSymmetry drs = new DetectReflectionalSymmetry();
            drs.Main($@"C:\Temp\Test.ply");
        }
    }

}

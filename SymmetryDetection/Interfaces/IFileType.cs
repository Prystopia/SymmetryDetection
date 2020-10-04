using System;
using System.Collections.Generic;
using System.Numerics;
using SymmetryDetection.DataTypes;

namespace SymmetryDetection.Interfaces
{
    public interface IFileType
    {
        void LoadFromFile(string fileLoc);

        PointCloud ConvertToPointCloud();
        
    }
}

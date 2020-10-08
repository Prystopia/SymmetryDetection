using SymmetryDetection.DataTypes;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.FileTypes
{
    public class ExperimentPointCloud : IFileType
    {
        public PointCloud ConvertToPointCloud()
        {
            throw new NotImplementedException();
        }

        public void LoadFromFile(string fileLoc)
        {
            throw new NotImplementedException();
        }
    }
}

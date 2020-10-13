using SymmetryDetection.DataTypes;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.e2e
{
    public class TestFileType : IFileType
    {
        private List<Vector3> Points { get; set; }
        public float SymmetryScore { get; set; }
        public TestFileType(List<Vector3> points)
        {
            this.Points = points;
        }

        public PointCloud ConvertToPointCloud()
        {
            PointCloud cloud = new PointCloud();

            foreach (var point in this.Points)
            {
                cloud.AddPoint(new PointXYZNormal() { Position = point });
            }
            return cloud;
        }

        public void LoadFromFile(string fileLoc)
        {
            //do nothing here
        }
    }
}

using SymmetryDetection.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    /// <summary>
    /// This class created a voxel grid, then all points within each voxel are approximated using their centroid to leave a single point in each voxel
    /// Mainly used to reduce the amount of data - not sure this is needed for now :)
    /// </summary>
    public class DownsampleService
    {
        public enum DownsampleMethod
        {
            Average
        }
        public PointCloud Cloud { get; set; }
        public DownsampleMethod Method { get; set; }
        public float LeafSize { get; set; }
        public DownsampleService(PointCloud cloud, DownsampleMethod method, float leafSize)
        {
            this.Cloud = cloud;
            this.Method = method;
            this.LeafSize = leafSize;
        }

        public PointCloud Filter()
        {
            return Cloud;
        }
    }
}

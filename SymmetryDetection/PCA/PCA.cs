using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.SymmetryDectection
{
    /// <summary>
    /// Holds meta data about the provided Point cloud
    /// </summary>
    public class PCA
    {
        public PointCloud Cloud { get; set; }
        public PointCloud DemeanedCloud { get; set; }
        private bool FakeIndices { get; set; }
        private List<int> Indices { get; set; }
        public Vector3 Mean { get; set; }

        public PCA(PointCloud pc)
        {
            this.Cloud = pc;
            this.FakeIndices = true;
            this.Indices = new List<int>();
            this.Mean = Vector3.Zero;
            for(int i = 0; i < Cloud.Points.Count; i++)
            {
                this.Indices.Add(i);
            }
            this.Initialise();
        }
        private void Initialise()
        {
            this.Calculate3DCentroid();
            this.DemeanCloud();
        }

        public void Calculate3DCentroid()
        {
            if(Cloud.IsDense)
            {
                float x = 0, y = 0, z = 0;
                foreach(var point in Cloud.Points)
                {
                    x += point.Position.X;
                    y += point.Position.Y;
                    z += point.Position.Z;
                }
                Mean = new Vector3(x, y, z);
                Mean /= Cloud.Points.Count;
            }
        }

        private void DemeanCloud()
        {
            DemeanedCloud = new PointCloud();
            DemeanedCloud.Height = Cloud.Height;
            DemeanedCloud.IsDense = Cloud.IsDense;
            foreach(var point in Cloud.Points)
            {
                var demeanedPoint = new PointXYZNormal();
                demeanedPoint.Normal = point.Normal;
                demeanedPoint.Position = new Vector3(point.Position.X - Mean.X, point.Position.Y - Mean.Y, point.Position.Z - Mean.Z);
                demeanedPoint.Colour = point.Colour;
                DemeanedCloud.AddPoint(demeanedPoint);
            }
        }

        private float[,] GetArray()
        {
            float[,] pc = new float[3, Indices.Count];



            return pc;
        }

    }
}

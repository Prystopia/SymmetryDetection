using Newtonsoft.Json;
using SymmetryDetection.DataTypes;
using SymmetryDetection.FileTypes.jsonData;
using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace SymmetryDetection.FileTypes
{
    public class InstallationDefinitionFile : IFileType
    {
        public List<ShapePosition> Items { get; set; }
        public float SymmetryLevel { get; set; }
        public Guid Identifier { get; set; }
        public float CompressionRatio { get; set; }
        public int TotalItems { get; set; }
        public float Roughness { get; set; }
        public float SYM3DLevel { get; set; }
        public float BoundingBoxArea { get; set; }
        public int VertexCount { get; set; }
        public int TotalFaces { get; set; }
        public int TotalEdges { get; set; }
        public string OffFileDef { get; set; }
        public string plyFile { get; set; }

        public PointCloud ConvertToPointCloud()
        {
            PointCloud cloud = new PointCloud();
            foreach (var vert in Items)
            {
                cloud.Points.Add(new PointXYZNormal()
                {
                    Position = new Vector3(vert.X, vert.Y, vert.Z)
                });
            }
            return cloud;
        }

        public void LoadFromFile(string fileLoc)
        {
            var objectDetails = JsonConvert.DeserializeObject<InstallationDefinitionFile>(File.ReadAllText(fileLoc));
            this.Items = objectDetails.Items;
        }
    }
}

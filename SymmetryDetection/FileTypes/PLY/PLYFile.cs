using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Interfaces;

namespace SymmetryDetection.FileTypes.PLY
{
   
    public class PLYFile : IFileType
    {
        private const string BINARY_TYPE = "format binary_little_endian 1.0";
        private const string ASCII_TYPE = "format ascii 1.0";
        private const string HEADER_END_TEXT = "end_header";
        private const string FILE_HEADER = "ply";
        private const string PROPERTY_IDENTIFIER = "property";
        private const string ELEMENT_IDENTIFIER = "element";
        private const string VERTEX_KEYWORD = "vertex";

        public int VertexCount { get; set; }
        public List<Vertex> Vertices { get; set; }
        public List<PLYProperty> Properties { get; set; }

        public bool IsBinary { get; set; }
        private int Width { get; set; }
        private int Height { get; set; }

        public PLYFile()
        {
            Properties = new List<PLYProperty>();
            Vertices = new List<Vertex>();
        }

        public void LoadFromFile(string fileLoc)
        {
            using (FileStream fs = new FileStream(fileLoc, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    ReadHeader(sr);
                    if (IsBinary)
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            ReadBody(br);
                        }
                    }
                    else
                    {
                        ReadBody(sr);
                    }
                }
            }
        }

        public PointCloud ConvertToPointCloud()
        {
            PointCloud cloud = new PointCloud();
            foreach (var vert in Vertices)
            {
                cloud.Points.Add(new PointXYZNormal()
                {
                    Position = vert.Position,
                    Normal = vert.Normal
                });
            }
            return cloud;
        }

        public static string WriteFile(List<Vector3> points)
        {
           var props = new List<PLYProperty>()
            {
                new PLYProperty(){ DataType = PLYProperty.PropertyType.Float, Name = "x" },
                new PLYProperty(){ DataType = PLYProperty.PropertyType.Float, Name = "y" },
                new PLYProperty(){ DataType = PLYProperty.PropertyType.Float, Name = "z" },
            };

            string header = WriteHeader(points.Count, props);
            string body = WriteBody(points);

            return $"{header}{body}";
        }

        private void ReadHeader(StreamReader fileReader)
        {
            int readCount = 0;
            string header = fileReader.ReadLine();
            readCount += header.Length + 1;
            if (header == FILE_HEADER)
            {
                string line = fileReader.ReadLine();
                readCount += line.Length + 1;
                IsBinary = line == BINARY_TYPE;

                line = fileReader.ReadLine();
                readCount += line.Length + 1;

                while (line != HEADER_END_TEXT)
                {
                    string[] col = line.Split();
                    if (col[0] == ELEMENT_IDENTIFIER)
                    {
                        if (col[1] == VERTEX_KEYWORD)
                        {
                            VertexCount = Convert.ToInt32(col[2]);
                        }
                    }
                    else if(col[0] == PROPERTY_IDENTIFIER)
                    {
                        string name = col[2];
                        var prop = new PLYProperty() { Name = name };
                        switch(col[1])
                        {
                            case "int":
                            case "uint":
                            case "int32":
                            case "uint32":
                                prop.DataType = PLYProperty.PropertyType.Int;
                                break;
                            case "char":
                            case "uchar":
                            case "int8":
                            case "uint8":
                                prop.DataType = PLYProperty.PropertyType.Byte;
                                break;
                            case "short":
                            case "ushort":
                            case "int16":
                            case "uint16":
                                prop.DataType = PLYProperty.PropertyType.Data16;
                                break;
                            case "int64":
                            case "uint64":
                            case "double":
                            case "float64":
                                prop.DataType = PLYProperty.PropertyType.Data64;
                                break;
                            case "float":
                            case "float32":
                                prop.DataType = PLYProperty.PropertyType.Float;
                                break;
                        }
                        Properties.Add(prop);
                    }

                    line = fileReader.ReadLine();
                    readCount += line.Length + 1;
                }
                fileReader.BaseStream.Position = readCount; //Not sure why
            }
        }

        private void ReadBody(BinaryReader fileReader)
        {
            for(int i = 0; i < VertexCount; i++)
            {

                Vector3 position = new Vector3(), normal = new Vector3(); 
                byte r = 255, g = 255, b = 255, a = 255;
                float curvature = 0;
                
                foreach(var property in Properties)
                {
                    switch(property.Name)
                    {
                        case "x":
                            position.X = fileReader.ReadSingle();
                            break;
                        case "y":
                            position.Y = fileReader.ReadSingle();
                            break;
                        case "z":
                            position.Z = fileReader.ReadSingle();
                            break;
                        case "red":
                            r = fileReader.ReadByte();
                            break;
                        case "green":
                            g = fileReader.ReadByte();
                            break;
                        case "blue":
                            b = fileReader.ReadByte();
                            break;
                        case "alpha":
                            a = fileReader.ReadByte();
                            break;
                        case "nx":
                            normal.X = fileReader.ReadSingle();
                            break;
                        case "ny":
                            normal.Y = fileReader.ReadSingle();
                            break;
                        case "nz":
                            normal.Z = fileReader.ReadSingle();
                            break;
                        case "curvature":
                            curvature = fileReader.ReadSingle();
                            break;
                        default:
                            switch(property.DataType)
                            {
                                case PLYProperty.PropertyType.Byte: fileReader.ReadByte(); break;
                                case PLYProperty.PropertyType.Data16: fileReader.BaseStream.Position += 2; break;
                                case PLYProperty.PropertyType.Int: fileReader.BaseStream.Position += 4; break;
                                case PLYProperty.PropertyType.Data64: fileReader.BaseStream.Position += 8; break;
                            }
                            break;
                    }
                    Vertices.Add(new Vertex()
                    {
                        Position = position,
                        Curvature = curvature,
                        Normal = normal
                    });
                }
            }
        }

        private void ReadBody(StreamReader fileReader)
        {
            for (int i = 0; i < VertexCount; i++)
            {

                Vector3 position = new Vector3(), normal = new Vector3();
                byte r = 255, g = 255, b = 255, a = 255;
                float curvature = 0;

                var line = fileReader.ReadLine();
                var items = line.Split(' ');
                int count = 0;

                foreach (var property in Properties)
                {
                    switch (property.Name)
                    {
                        case "x":
                            position.X = float.Parse(items[count]);
                            break;
                        case "y":
                            position.Y = float.Parse(items[count]);
                            break;
                        case "z":
                            position.Z = float.Parse(items[count]);
                            break;
                        case "red":
                            r = byte.Parse(items[count]);
                            break;
                        case "green":
                            g = byte.Parse(items[count]);
                            break;
                        case "blue":
                            b = byte.Parse(items[count]);
                            break;
                        case "alpha":
                            a = byte.Parse(items[count]);
                            break;
                        case "nx":
                            normal.X = float.Parse(items[count]);
                            break;
                        case "ny":
                            normal.Y = float.Parse(items[count]);
                            break;
                        case "nz":
                            normal.Z = float.Parse(items[count]);
                            break;
                        case "curvature":
                            curvature = float.Parse(items[count]);
                            break;
                    }
                    count++;
                }
                Vertices.Add(new Vertex()
                {
                    Position = position,
                    Curvature = curvature,
                    Normal = normal
                });
            }
        }



        private static string WriteHeader(int vertexCount, List<PLYProperty> properties)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FILE_HEADER);
            sb.Append(Environment.NewLine);
            sb.Append(ASCII_TYPE);
            sb.Append(Environment.NewLine);
            sb.Append($"{ELEMENT_IDENTIFIER} {VERTEX_KEYWORD} {vertexCount}");
            sb.Append(Environment.NewLine);

            foreach(var property in properties)
            {
                sb.Append($"{PROPERTY_IDENTIFIER} {property.DataType.ToString().ToLower()} {property.Name}");
                sb.Append(Environment.NewLine);
            }

            sb.Append(HEADER_END_TEXT);
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
        private static string WriteBody(List<Vector3> verts)
        {
            StringBuilder sb = new StringBuilder();

            foreach(var vertex in verts)
            {
                sb.Append($"{vertex.X} {vertex.Y} {vertex.Z}");
                sb.Append(Environment.NewLine);
            }    


            return sb.ToString();
        }
    }
}

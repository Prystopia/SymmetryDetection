using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.FileTypes
{
    public class PLYProperty
    {
        public enum PropertyType
        {
            Float,
            Int,
            Byte,
            Data16,
            Data64
        }
        public PropertyType DataType { get; set; }
        public string Name { get; set; }
    }
    public class Vertex
    {
        public Vector3 Position { get; set; }
        public Color Colour { get; set; }
        public Vector3 Normal { get; set; }
        public float Curvature { get; set; }
        public Dictionary<string, object> AdditionalProperties { get; set; }
    }
    public class PLYFile
    {
        private const string BINARY_TYPE = "format binary_little_endian 1.0";
        private const string HEADER_END_TEXT = "end_header";
        private const string MAGIC_STRING = "ply";
        private const string PROPERTY_IDENTIFIER = "property";
        private const string ELEMENT_IDENTIFIER = "element";
        private const string VERTEX_KEYWORD = "vertex";

        public Vector4 Origin { get; set; }
        public Quaternion Orientation { get; set; }
        public int VertexCount { get; set; }
        public List<List<int>> RangeGrid { get; set; }
        public List<Vertex> Vertices { get; set; }
        public List<PLYProperty> Properties { get; set; }

        public bool IsBinary { get; set; }
        private int Width { get; set; }
        private int Height { get; set; }



        public PLYFile()
        {
            Origin = Vector4.Zero;
            Orientation = Quaternion.Identity;
            Properties = new List<PLYProperty>();
            Vertices = new List<Vertex>();
        }

        public void ReadFile(string fileLoc)
        {
            using (FileStream fs = new FileStream(fileLoc, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    ReadHeader(sr);
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        ReadBody(br);
                    }
                }
            }
        }

        private void ReadHeader(StreamReader fileReader)
        {
            int readCount = 0;
            string header = fileReader.ReadLine();
            readCount += header.Length + 1;
            if (header == MAGIC_STRING)
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
                        var prop = new PLYProperty() { Name = col[2] };
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
                            position.X = fileReader.ReadSingle();
                            break;
                        case "z":
                            position.X = fileReader.ReadSingle();
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
                        Colour = Color.FromArgb(r, b, g, a),
                        Position = position,
                        Curvature = curvature,
                        Normal = normal
                    });
                }
            }
        }

    }
}

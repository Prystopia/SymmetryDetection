using System;
namespace SymmetryDetection.FileTypes.PLY
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
}

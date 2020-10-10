using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Exporters
{
    public class JsonExporter : ISymmetryExporter
    {
        public string ExportSymmetries(IList<ISymmetry> symmetries, float? globalScore = null)
        {
            throw new NotImplementedException();
        }
    }
}

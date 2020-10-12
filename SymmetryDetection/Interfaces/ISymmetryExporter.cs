using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Interfaces
{
    public interface ISymmetryExporter
    {
        string ExportSymmetries(IList<ISymmetry> symmetries, float? symmetryScore = null);
    }
}

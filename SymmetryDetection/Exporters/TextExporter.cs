using SymmetryDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Exporters
{
    public class TextExporter : ISymmetryExporter
    {
        public string ExportSymmetries(IList<ISymmetry> symmetries, float? symmetryScore = null)
        {
            StringBuilder sb = new StringBuilder();
            int count = 1;
            sb.Append($"{symmetries.Count} symmetries detected");
            sb.Append(Environment.NewLine);
            if (symmetryScore != null)
            {
                sb.Append($"Global Score: {symmetryScore}");
                sb.Append(Environment.NewLine);
            }
            foreach (var symm in symmetries)
            {
                sb.Append($"{count}: Origin = X: {Math.Round(symm.Origin.X, 2)}, Y: {Math.Round(symm.Origin.Y, 2)}, Z: {Math.Round(symm.Origin.Z, 2)}; Normal = X: {Math.Round(symm.Normal.X, 2)}, Y: {Math.Round(symm.Normal.Y, 2)}, Z: {Math.Round(symm.Normal.Z, 2)};");
                sb.Append(Environment.NewLine);
                count++;
            }

            return sb.ToString();
        }
    }
}

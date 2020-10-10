using SymmetryDetection.DataTypes;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SymmetryDetection.Refinement
{
    public class RotationalSymmetryRefinementFunction : ILMFunction
    {
        public PointCloud Cloud { get; set; }
        public float MaxFitAngle { get; set; }

        public RotationalSymmetry Symmetry { get; set; }

        public int InputSize => 6;

        public int ValuesSize => Cloud.Points.Count;

        public double[] Function(double[] input)
        {
            double[] errors = new double[ValuesSize];

            for (int i = 0; i < Cloud.Points.Count; i++)
            {
                var point = Cloud.Points[i];
                double angle = Symmetry.GetRotationalFitError(point.Position, point.Normal);
                errors[i] = Math.Min(angle, MaxFitAngle);
            }
            return errors;
        }
    }
}

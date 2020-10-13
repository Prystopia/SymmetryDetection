using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.Helpers;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.Refinement
{
    public class ReflectionalSymmetryRefinementFunction : IOptimisationFunction
    {
        private PointCloud Cloud { get; set; }
        public List<Correspondence> Correspondences { get; set; }
        public int ValuesSize => Correspondences.Count;
        public int InputSize => 6;
        public ReflectionalSymmetryRefinementFunction(PointCloud cloud)
        {
            Cloud = cloud;
        }

        //input data could be the correspondences [src point, target point] - however we're allowing more flexibility in the function to minimise but storing data as part of this class
        public double[] Function(double[] input)
        {
            double[] errors = new double[Correspondences.Count];

            Vector3 origin = new Vector3((float)input[0], (float)input[1], (float)input[2]);
            Vector3 normal = new Vector3((float)input[3], (float)input[4], (float)input[5]);

            //this is our initial guess ^^

            ReflectionalSymmetry sym = new ReflectionalSymmetry(origin, normal);

            for (int i = 0; i < Correspondences.Count; i++)
            {
                var point = Correspondences[i];
                Vector3 sourcePoint = point.Original.Position;
                //Vector3 sourceNormal = point.Original.Normal;

                Vector3 targetPoint = point.CorrespondingPoint.Position;
                Vector3 targetNormal = point.CorrespondingPoint.Normal;

                Vector3 targetPointReflected = PointHelpers.ReflectPoint(targetPoint,sym);
                Vector3 targetNormalReflected = PointHelpers.ReflectNormal(targetNormal, sym);

                errors[i] = MathF.Abs(MathsHelpers.PointToPlaneSignedDistance(sourcePoint, targetPointReflected, targetNormalReflected));
            }
            return errors;
            
        }
    }
}

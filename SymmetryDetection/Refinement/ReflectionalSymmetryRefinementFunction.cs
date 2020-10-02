using SymmetryDetection.DataTypes;
using SymmetryDetection.Extensions;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.Refinement
{
    public interface LMFunction
    {
        float[] Function(float[] input);
        //int df(float[] input, float[,] fjac);
        int InputSize { get; }
        int ValuesSize { get; }

    }
    public class ReflectionalSymmetryRefinementFunction : LMFunction
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
        public float[] Function(float[] input)
        {
            float[] errors = new float[Correspondences.Count];
            Vector6 pos = new Vector6()
            {
                X = input[0],
                Y = input[1],
                Z = input[2],
                A = input[3],
                B = input[4],
                C = input[5]
            };
            //this is our initial guess ^^

            ReflectionalSymmetry sym = new ReflectionalSymmetry(pos.Head, pos.Tail);

            for (int i = 0; i < Correspondences.Count; i++)
            {
                var point = Correspondences[i];
                Vector3 sourcePoint = point.Original.Position;
                //Vector3 sourceNormal = point.Original.Normal;

                Vector3 targetPoint = point.CorrespondingPoint.Position;
                Vector3 targetNormal = point.CorrespondingPoint.Normal;

                Vector3 targetPointReflected = sym.ReflectPoint(targetPoint);
                Vector3 targetNormalReflected = sym.ReflectNormal(targetNormal);

                errors[i] = MathF.Abs(ReflectionalSymmetryDetection.PointToPlaneSignedDistance(sourcePoint, targetPointReflected, targetNormalReflected));
            }
            return errors;
            
        }

        //public int df(float[] input, float[,] jac)
        //{
        //    float h;
        //    int nfev = 0;
        //    int n = InputSize;
        //    float eps = MathF.Sqrt(MathF.Max(0, ExtensionMethods.EPSILON));
        //    float[] val1, val2;
        //    float[] x = input;
        //    // TODO : we should do this only if the size is not already known
        //    val1 = new float[ValuesSize];
        //    val2 = new float[ValuesSize];

        //    Function(x, val1);

        //    // Function Body
        //    for (int j = 0; j < n; ++j)
        //    {
        //        h = eps * MathF.Abs(x[j]);
        //        if (h == 0)
        //        {
        //            h = eps;
        //        }
        //        x[j] += h;
        //        Function(x, val2);
        //        nfev++;
        //        x[j] = input[j];
        //        jac.SetColumn(j, val2.Minus(val1).Divide(h));
        //    }
        //    return nfev;
        //}
    }

}

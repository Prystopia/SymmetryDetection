using SymmetryDetection.DataTypes;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SymmetryDetection.Extensions
{
    public static class ExtensionMethods
    {
        public const float EPSILON = 2.2204460492503131e-16f;
        public static Vector3 MultiplyVector(this Vector3 vector, float[,] matrix)
        {
            Vector3 newVector = new Vector3();
            newVector.X = (vector.X * matrix[0, 0]) + (vector.Y * matrix[0, 1]) + (vector.Z * matrix[0, 2]);
            newVector.Y = (vector.X * matrix[1, 0]) + (vector.Y * matrix[1, 1]) + (vector.Z * matrix[1, 2]);
            newVector.Z = (vector.X * matrix[2, 0]) + (vector.Y * matrix[2, 1]) + (vector.Z * matrix[2, 2]);
            return newVector;
        }

        public static Vector3 GetCol(this float[,] matrix, int column)
        {
            return new Vector3(matrix[0, column], matrix[1, column], matrix[2, column]);
        }

        public static float[] GetColumn(this float[,] matrix, int column)
        {
            float[] val = new float[matrix.GetLongLength(column)];
            for (int i = 0; i < matrix.GetLongLength(column); i++)
            {
                val[i] = matrix[i, column];
            }

            return val;
        }
        public static void SetColumn(this float[,] matrix, int column, float[] newValue)
        {
            for (int i = 0; i < matrix.GetLongLength(column); i++)
            {
                matrix[i, column] = newValue[i];
            }
        }

        public static float[] GetHead(this float[] matrix, int numToReturn)
        {
            float[] returnVal = new float[numToReturn];
            for(int i = 0; i < numToReturn; i++)
            {
                returnVal[i] = matrix[i];
            }
            return returnVal;
        }

        public static float[,] ConvertToArray(this PointCloud pointCloud)
        {
            float[,] pc = new float[3, pointCloud.Points.Count];

            for (int i = 0; i < pointCloud.Points.Count; i++)
            {
                var point = pointCloud.Points[i];
                pc[0, i] = point.Position.X;
                pc[1, i] = point.Position.Y;
                pc[2, i] = point.Position.Z;
            }

            return pc;
        }

        public static float ClampValue(this float val, float min, float max)
        {
            float bounded = val;
            bounded = Math.Max(bounded, min);
            bounded = Math.Min(bounded, max);
            return bounded;
        }

        public static float ConvertToRadians(this float angle)
        {
            return (MathF.PI / 180) * angle;
        }

        public static string GetExportFile(this List<ReflectionalSymmetry> symmetries)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SYMMETRY_ID,ORIGIN,NORMAL");
            sb.Append(Environment.NewLine);

            for (int i = 0; i < symmetries.Count; i++)
            {
                var sym = symmetries[i];
                sb.Append($"{i},");
                sb.Append($"X: {sym.Origin.X} Y: {sym.Origin.Y} Z: {sym.Origin.Z},");
                sb.Append($"X: {sym.Normal.X} Y: {sym.Normal.Y} Z: {sym.Normal.Z},");
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public static float StableNorm(this float[] array)
        {
            int index = array.Length;

            float scale = 0;
            float inverseScale = 1;
            float ssq = 0;

            float maxCoefficient = array.AbsoluteValue().MaxCoefficient();
            if (maxCoefficient > scale)
            {
                ssq = ssq * Math.Abs(scale / maxCoefficient);
                float temp = 1f / maxCoefficient;
                if (float.IsInfinity(temp))
                {
                    inverseScale = float.MaxValue;
                    scale = 1 / inverseScale;
                }
                else if (float.IsInfinity(maxCoefficient))
                {
                    inverseScale = 1;
                    scale = maxCoefficient;
                }
                else
                {
                    scale = maxCoefficient;
                    inverseScale = temp;
                }
            }
            else
            {
                scale = maxCoefficient;
            }

            if (scale > 0f)
            {
                ssq +=
            }

            return scale = MathF.Sqrt(ssq);
        }

        public static float BlueNorm(this float[] array)
        {
            // This program calculates the machine-dependent constants
            // bl, b2, slm, s2m, relerr overfl
            // from the "basic" machine-dependent numbers
            //nbig, ibeta, it, iemin, iemax, rbig.
            // The following define the basic machine-dependent constants.
            // For portability, the PORT subprograms "ilmaeh" and "rlmach"
            // are used. For any specific computer, each of the assignment
            // statements can be replaced

            int ibeta = std::numeric_limits < RealScalar >::radix;  // base for floating-point numbers
            int it = NumTraits < RealScalar >::digits();  // number of base-beta digits in mantissa
            int iemin = std::numeric_limits < RealScalar >::min_exponent;  // minimum exponent
            int iemax = std::numeric_limits < RealScalar >::max_exponent; // maximum exponent
            RealScalar rbig = (std::numeric_limits<RealScalar>::max)();  // largest floating-point number
            float b1 = RealScalar(pow(RealScalar(ibeta), RealScalar(-((1 - iemin) / 2))));  // lower boundary of midrange
            float b2 = RealScalar(pow(RealScalar(ibeta), RealScalar((iemax + 1 - it) / 2)));  // upper boundary of midrange
            RealScalar s1m = RealScalar(pow(RealScalar(ibeta), RealScalar((2 - iemin) / 2)));  // scaling factor for lower range
            RealScalar s2m = RealScalar(pow(RealScalar(ibeta), RealScalar(-((iemax + it) / 2))));  // scaling factor for upper range
            RealScalar eps = RealScalar(pow(double(ibeta), 1 - it));
            RealScalar relerr = sqrt(eps);  // tolerance for neglecting asml

            //            const Derived&vec(_vec.derived());
            int n = array.Length;
            float ab2 = b2 / n;
            float asml = 0;
            float amed = 0;
            float abig = 0;

            for (int j = 0; j < array.Length; j++)
            {
                float val = array[j];

                //this is the definition of abs2
                float ax = MathF.Abs(val);
                if (ax > ab2)
                {
                    abig += MathF.Abs(MathF.Pow(ax * s2m, 2));
                }
                else if (ax < b1)
                {
                    asml += MathF.Abs(MathF.Pow(ax * s1m, 2));
                }
                else
                {
                    amed += MathF.Abs(MathF.Pow(ax, 2));
                }
            }
            if(float.IsNaN(amed))
            {
                return amed;
            }

            if(abig > 0)
            {
                abig = MathF.Sqrt(abig);
                if(abig > rbig)
                {
                    return abig;
                }
                
                if(amed > 0)
                {
                    abig = abig / s2m;
                    amed = MathF.Sqrt(amed);
                }
                else
                {
                    return abig / s2m;
                }
            }
            else if(asml > 0)
            {
                if(amed > 0)
                {
                    abig = MathF.Sqrt(amed);
                    amed = MathF.Sqrt(asml) / s1m;
                }
                else
                {
                    return MathF.Sqrt(asml) / s1m;
                }
            }
            else
            {
                return MathF.Sqrt(amed);
            }

            asml = MathF.Min(abig, amed);
            abig = MathF.Max(abig, amed);

            if (asml <= abig * relerr)
                return abig;
            else
                return abig * MathF.Sqrt(1 + MathF.Abs(MathF.Pow(asml / abig, 2));
        }

        public static float[] AbsoluteValue(this float[] norm)
        {
            float[] absoluteArray = new float[norm.Length];
            for(int i = 0; i < norm.Length; i++)
            {
                absoluteArray[i] = MathF.Abs(norm[i]);
            }
            return absoluteArray;
        }

        public static float MaxCoefficient(this float[] value)
        {
            return value.Max();
        }

        public static float Max(this float[] value)
        {
            return value.Max();
        }

        public static float[] CrosswiseProduct(this float[] val1, float[] val2)
        {
            float[] returnValue = new float[val1.Length];

            for (int i = 0; i < val1.Length; i++)
            {
                returnValue[i] = val1[i] * val2[i];
            }

            return returnValue;
        }

        public static float[] AddArray(this float[] val1, float[] val2)
        {
            float[] returnValue = new float[val1.Length];

            for (int i = 0; i < val1.Length; i++)
            {
                returnValue[i] = val1[i] + val2[i];
            }

            return returnValue;
        }

        public static float[] Divide(this float[] val1, float val2)
        {
            float[] returnValue = new float[val1.Length];

            for (int i = 0; i < val1.Length; i++)
            {
                returnValue[i] = val1[i] / val2;
            }

            return returnValue;
        }

        public static float[] CrosswiseMax(this float[] val, float[] val2)
        {
            float[] returnVal = new float[val.Length];
            for(int i = 0; i < val.Length; i++)
            {
                returnVal[i] = MathF.Max(val[i], val2[i]);
            }

            return returnVal;
        }

        public static float[] Minus(this float[] item1, float[] item2)
        {
            float[] returnVal = new float[item1.Length];
            for (int i = 0; i < item1.Length; i++)
            {
                returnVal[i] = item1[i] - item2[i];
            }

            return returnVal;
        }

        public static int[] GetIndices(this bool[,] initial)
        {
            int[] indices = new int[initial.Length];

            for(int i = 0; i < initial.Length; i++)
            {
                indices[i] = i;
            }

            return indices;
        }

        public static float[,] Adjoint(this float[,] array)
        {
            //Conjugate transpose is the adjoint
            //however not dealing with imaginary numbers (As far as I'm aware) and the conjugate of a real number is just the same number
            //so can just return the transposed array
            return Transpose(array);
        }

        public static bool[,] Inverse(this bool[,] array)
        {
            bool[,] newArray = new bool[array.GetLength(0), array.GetLength(1)];

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    newArray[i, j] = !array[i, j];
                }
            }
            return newArray;
        }

        

        public static float[,] Transpose(this float[,] original)
        {
            float[,] newArray = new float[original.GetLength(1), original.Length];

            for(int i = 0; i < original.Length; i++)
            {
                for(int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[j, i] = original[i, j];
                }
            }


            return newArray;
        }

    }
}

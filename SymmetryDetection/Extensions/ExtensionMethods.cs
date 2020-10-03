using Microsoft.Win32.SafeHandles;
using SymmetryDetection.DataTypes;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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

        public static float[,] TopRows(this float[,] original, int numRows)
        {
            float[,] item = new float[numRows, original.GetLength(1)];

            for(int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    item[i, j] = original[i, j];
                }
            }

            return item;
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
            for (int i = 0; i < numToReturn; i++)
            {
                returnVal[i] = matrix[i];
            }
            return returnVal;
        }
        public static float[] SetHead(this float[] matrix, int numToSet, float value)
        {
            float[] returnVal = matrix;
            for (int i = 0; i < numToSet; i++)
            {
                returnVal[i] = value;
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

        public static float Dot(this Vector3 original, Vector3 next)
        {
            float val = 0;

            var x = original.X * next.X;
            var y = original.Y * next.Y;
            var z = original.Z * next.Z;
            val = x + y + z;

            return val;
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
            float norm = -1;

            if (index == 1)
            {
                norm = MathF.Abs(array[0]);
            }
            else
            {
                float scale = 0;
                float inverseScale = 1;
                float ssq = 0;

                float maxCoefficient = array.AbsoluteValue().MaxCoefficient(out _);
                if (maxCoefficient > scale)
                {
                    ssq = ssq * MathF.Abs(scale / maxCoefficient);
                    float temp = 1f / maxCoefficient;
                    if (temp > float.MaxValue)
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
                    ssq += array.Multiply(inverseScale).SquaredNorm();
                }

                norm = scale = MathF.Sqrt(ssq);
            }
            return norm;
        }

        public static float SquaredNorm(this float[] original)
        {
            return original.Sum().Abs2();
        }

        public static float Abs2(this float value)
        {
            return MathF.Abs(MathF.Pow(value, 2));
        }

        public static float Dot(this float[] original, float[] additional)
        {
            float sum = 0;
            //multiply first row by first column
            for (int i = 0; i < original.Length; i++)
            {
                sum += original[i] * additional[i];
            }

            return sum;
        }

        public static bool[,] GetInverse(this bool[,] original)
        {
            bool[,] newArray = new bool[original.GetLength(0), original.GetLength(1)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = !original[i, j];
                }
            }

            return newArray;
        }

        public static float[] Multiply(this float[] original, float factor)
        {
            float[] newArray = new float[original.Length];

            for (int i = 0; i < original.Length; i++)
            {
                newArray[i] = original[i] * factor;
            }

            return newArray;
        }
 
        public static float[] Tail(this float[] original, int numToTake)
        {
            float[] newArray = new float[numToTake];

            for (int i = 0; i < numToTake; i++)
            {
                newArray[i] = original[original.Length - numToTake + i];
            }

            return newArray;
        }

        public static float[] SetTail(this float[] original, int numToTake, int value)
        {
            float[] newArray = new float[original.Length];

            for (int i = 0; i < original.Length; i++)
            {
                newArray[i] = (i >= original.Length - numToTake + i) ? value : original[i];
            }

            return newArray;
        }

        public static float[] Multiply(this bool[,] permutation, float[] original)
        {
            float[] newArray = new float[original.Length];
            //based off left multiplication - rearranges corresponding rows
            for(int i = 0; i < permutation.GetLength(0); i++)
            {
                //find indexes of the true values [i,j]
                //i = row to move original row to
                //j = row index to move
                for(int j = 0; j < permutation.GetLength(1); j++)
                {
                    if(permutation[i,j])
                    {
                        newArray[i] = original[j];
                    }
                }
            }
            return newArray;
        }

        public static float[] GetDiagonal(this float[,] original)
        {
            float[] newArray = new float[(original.GetLength(0)  * original.GetLength(1)) / 2];
            int count = 0;
            //based off left multiplication - rearranges corresponding rows
            for (int i = 0; i < original.GetLength(0); i++)
            {
                //find indexes of the true values [i,j]
                //i = row to move original row to
                //j = row index to move
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    if(i == j)
                    {
                        newArray[count] = original[i, j];
                        count++;
                    }

                }
            }
            return newArray;
        }

        public static void SetDiagonal(this float[,] original, float[] values)
        {
            int count = 0;
            //should really check that length of values is correct
            for(int i = 0; i < original.GetLength(0); i++)
            {
                for(int j = 0; j < original.GetLength(1); j++)
                {
                    if(i == j)
                    {
                        original[i, j] = values[count];
                        count++;
                    }
                }
            }
        }
        //public static float[,] BottomRightCorner(this float[,] original, int height, int width)
        //{

        //}

        //public static void MakeHouseholderInPlace(this float[] original, float tau, out float beta)
        //{

        //}

        //public static void ApplyHouseholderOnTheLeft(this float[,] original, float[] something, float tau, out float beta)
        //{

        //}

        //public static void ApplyTranspositionOnTheRight(this bool[,] original, int val1, float val2)
        //{
        //    TODO
        //}
        public static float[,] Multiply(this float[,] original, float factor)
        {

            float[,] newArray = new float[original.GetLength(0), original.GetLength(1)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i,j] = original[i, j] * factor;
                }
            }

            return newArray;
        }

        //public static float[,] TopLeftCorner(this float[,] original, int width, int height)
        //{

        //}
        public static float[,] Sub(this float[,] original, float[,] value)
        {
            //if (original.GetLength(0) != value.GetLength(0) || this.columns !== matrix.columns)
            //{
            //    throw new RangeError('Matrices dimensions must be equal');
            //}
            float[,] newArray = new float[original.GetLength(0), original.GetLength(1)];
            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = original[i, j] - value[i, j];
                }
            }
            return newArray;
        }

        public static float[] To1DArray(this float[,] original)
        {
            List<float> array = new List<float>();
            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    array.Add(original[i, j]);
                }
            }
            return array.ToArray();
        }
        public static float[,] MatrixMultiply(this float[,] original, float[][] other)
        {
            int m = original.GetLength(0);
            int n = original.GetLength(1);
            int p = other[0].GetLength(0);

            float[,] result = new float[m, p];

            float[] Bcolj = new float[n];
            for (int j = 0; j < p; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    Bcolj[k] = other[k][j];
                }

                for (int i = 0; i < m; i++)
                {
                    float s = 0;
                    for (int k = 0; k < n; k++)
                    {
                        s += original[i, k] * Bcolj[k];
                    }

                    result[i, j] = s;
                }
            }
            return result;
        }

        public static float[,] MatrixMultiply(this float[,] original, float[,] other)
        {
            int m = original.GetLength(0);
            int n = original.GetLength(1);
            int p = other.GetLength(1);

            float[,] result = new float[m, p];

            float[] Bcolj = new float[n];
            for (int j = 0; j < p; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    Bcolj[k] = other[k,j];
                }

                for (int i = 0; i < m; i++)
                {
                    float s = 0;
                    for (int k = 0; k < n; k++)
                    {
                        s += original[i, k] * Bcolj[k];
                    }

                    result[i, j] = s;
                }
            }
            return result;
        }



        public static float[,] MatrixMultiply(this float[][] original, float[,] other)
        {
            int m = original.GetLength(0);
            int n = original[0].GetLength(0);
            int p = other.GetLength(1);

            float[,] result = new float[m, p];

            float[] Bcolj = new float[n];
            for (int j = 0; j < p; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    Bcolj[k] = other[k,j];
                }

                for (int i = 0; i < m; i++)
                {
                    float s = 0;
                    for (int k = 0; k < n; k++)
                    {
                        s += original[i][k] * Bcolj[k];
                    }

                    result[i, j] = s;
                }
            }
            return result;
        }
        public static float[][] MatrixMultiply(this float[][] original, float[][] other)
        {
            int m = original.GetLength(0);
            int n = original[0].GetLength(0);
            int p = other[0].GetLength(0);

            float[][] result = new float[m][];

            float[] Bcolj = new float[n];
            for (int j = 0; j < p; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    Bcolj[k] = other[k][j];
                }

                for (int i = 0; i < m; i++)
                {
                    if (result[i] == null)
                    {
                        result[i] = new float[m];
                    }
                    float s = 0;
                    for (int k = 0; k < n; k++)
                    {
                        s += original[i][k] * Bcolj[k];
                    }

                    result[i][j] = s;
                }
            }
            return result;
        }

        public static float FrobiusNorm(this float[] array)
        {
            //square root of sum of the absolute squares of the elements

            float sum = 0;

            for(int i = 0; i < array.Length; i++)
            {
                sum += Abs2(array[i]);
            }

            return MathF.Sqrt(sum);
        }

        public static float[,] Multiply(this float[,] original, float[,] other)
        {
            int m = original.GetLength(0);
            int n = original.GetLength(1);
            int p = other.GetLength(1);

            float[,] result = new float[m, p];

            float[] Bcolj = new float[n];
            for (int j = 0; j < p; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    Bcolj[k] = other[k,j];
                }

                for (int i = 0; i < m; i++)
                {
                    float s = 0;
                    for (int k = 0; k < n; k++)
                    {
                        s += original[i, k] * Bcolj[k];
                    }

                    result[i, j] = s;
                }
            }
            return result;
        }

        public static float[,] SubMatrix(this float[,] original, int startRow, int endRow, int startCol, int endCol)
        {
            float[,] newMatrix = new float[endRow - startRow + 1, endCol - startCol + 1];
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startCol; j <= endCol; j++)
                {
                    newMatrix[i - startRow, j - startCol] = original[i, j];
                }
            }
            return newMatrix;
        }

        //public static void Fill(this float[] original, float value)
        //{
        //    for(int i = 0; i < original.Length; i++)
        //    {
        //        original[i] = value;
        //    }
        //}

        public static void SwapCol(this float[,] original, int originalColIndex, int replacementColIndex)
        {
            var temp = original.GetColumn(originalColIndex);

            for(int i = 0; i < original.GetLength(0); i++)
            {
                original[i, originalColIndex] = original[i, replacementColIndex];
                original[i, replacementColIndex] = temp[i];
            }
        }

        public static float[] SetZero(this float[] original)
        {
            float[] newArray = new float[original.Length];

            for (int i = 0; i < original.Length; i++)
            {
                newArray[i] = 0;
            }
            return newArray;
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

            int ibeta = 2;// (radix) base for floating-point numbers
            int it = 24;  // number of base-beta digits in mantissa
            int iemin = -125; // minimum float exponent
            int iemax = 128; // maximum float exponent
            float rbig = float.MaxValue; // largest floating-point number
            float b1 = MathF.Pow(ibeta, -((1 - iemin) / 2));  // lower boundary of midrange
            float b2 = MathF.Pow(ibeta, (iemax + 1 - it) / 2);  // upper boundary of midrange
            float s1m = MathF.Pow(ibeta, (2 - iemin) / 2);  // scaling factor for lower range
            float s2m = MathF.Pow(ibeta, -((iemax + it) / 2));  // scaling factor for upper range
            float eps = MathF.Pow(ibeta, 1 - it);
            float relerr = MathF.Sqrt(eps);  // tolerance for neglecting asml

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
            if (float.IsNaN(amed))
            {
                return amed;
            }

            if (abig > 0)
            {
                abig = MathF.Sqrt(abig);
                if (abig > rbig)
                {
                    return abig;
                }

                if (amed > 0)
                {
                    abig = abig / s2m;
                    amed = MathF.Sqrt(amed);
                }
                else
                {
                    return abig / s2m;
                }
            }
            else if (asml > 0)
            {
                if (amed > 0)
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
                return abig * MathF.Sqrt(1 + MathF.Abs(MathF.Pow(asml / abig, 2)));
        }

        public static float[] AbsoluteValue(this float[] norm)
        {
            float[] absoluteArray = new float[norm.Length];
            for (int i = 0; i < norm.Length; i++)
            {
                absoluteArray[i] = MathF.Abs(norm[i]);
            }
            return absoluteArray;
        }

        public static float MaxCoefficient(this float[] value, out int colIndex)
        {
            var max = value.Max();
            colIndex = -1;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == max)
                {
                    colIndex = i;
                    break;
                }
            }
            return max;
        }

        public static float Norm(this float[] original)
        {
            return original.Sum();
        }

        public static float[,] UpperTrianglarView(this float[,] original)
        {
            float[,] newArray = new float[original.GetLength(0), original.GetLength(1)];

            for(int i = 0; i < original.GetLength(0); i++)
            {
                for(int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = (i <= j) ? original[i, j] : 0; 
                }
            }

            return newArray;
        }

        public static float[,] LowerTrianglarView(this float[,] original)
        {
            float[,] newArray = new float[original.GetLength(0), original.GetLength(1)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = (i <= j) ? 0 : original[i, j];
                }
            }

            return newArray;
        }

        public static T[,] GetStrictlyLowerView<T>(this T[,] original)
        {
            T[,] newArray = new T[original.GetLength(0), original.GetLength(1)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = (i < j) ? default(T) : original[i, j];
                }
            }

            return newArray;
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
            float[,] newArray = new float[original.GetLength(1), original.GetLength(0)];

            for(int i = 0; i < original.GetLength(0); i++)
            {
                for(int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[j, i] = original[i, j];
                }
            }


            return newArray;
        }

        public static float[][] Transpose(this float[][] original)
        {
            float[][] newArray = new float[original[0].GetLength(0)][];

            for (int j = 0; j < original[0].GetLength(0); j++)
            {
                newArray[j] = new float[original.GetLength(0)];
                for (int i = 0; i < original.GetLength(0); i++)
                {
                    newArray[j][i] = original[i][j];
                }
            }


            return newArray;
        }
    }
}

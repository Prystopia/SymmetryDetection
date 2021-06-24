using SymmetryDetection.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SymmetryDetection.Extensions
{
    public static class ExtensionMethods
    {
        public const float EPSILON = 2.2204460492503131e-16f;

        public static float[] GetColumn(this float[,] matrix, int column)
        {
            float[] val = new float[matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                val[i] = matrix[i, column];
            }

            return val;
        }

        public static double[] GetColumn(this double[,] matrix, int column)
        {
            double[] val = new double[matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                val[i] = matrix[i, column];
            }

            return val;
        }

        public static void Fill(this double[,] matrix, double value)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = value;
                }
            }
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
            return (float)(Math.PI / 180) * angle;
        }

        public static double[] GetRow(this double[,] original, int row)
        {
            double[] newArray = new double[original.GetLength(1)];

            for (int i = 0; i < original.GetLength(1); i++)
            {
                newArray[i] = original[row, i];
            }

            return newArray;
        }

        public static float SquaredNorm(this float[] original)
        {
            return original.Sum().Abs2();
        }

        public static double SquaredNorm(this double[] original)
        {
            return Math.Sqrt(original.Select(x => Math.Pow(x, 2)).Sum());
        }

        public static float Abs2(this float value)
        {
            return (float)Math.Abs(Math.Pow(value, 2));
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

        public static double[,] Multiply(this double[,] original, double factor)
        {
            double[,] newArray = new double[original.GetLength(0), original.GetLength(1)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = original[i,j] * factor;
                }
            }

            return newArray;
        }

        public static Vector3 Multiply(this float[,] original, Vector3 factor)
        {
            if (original.GetLength(1) != 3 && original.GetLength(0) != 3)
            {
                throw new ArgumentException();
            }
            Vector3 newVector = new Vector3();
            newVector.X = factor.X * original[0, 0] + factor.Y * original[0, 1] + factor.Z * original[0, 2];
            newVector.Y = factor.X * original[1, 0] + factor.Y * original[1, 1] + factor.Z * original[1, 2];
            newVector.Z = factor.X * original[2, 0] + factor.Y * original[2, 1] + factor.Z * original[2, 2];
            return newVector;
        }

        public static double[,] Subtract(this double[,] original, double[,] value)
        {
            double[,] newArray = new double[original.GetLength(0), original.GetLength(1)];
            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = original[i, j] - value[i, j];
                }
            }
            return newArray;
        }

        public static T[] To1DArray<T>(this T[,] original)
        {
            List<T> array = new List<T>();
            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    array.Add(original[i, j]);
                }
            }
            return array.ToArray();
        }

        public static double[,] Multiply(this double[,] original, double[,] other)
        {
            int m = original.GetLength(0);
            int n = original.GetLength(1);
            int p = other.GetLength(1);

            double[,] result = new double[m, p];

            double[] Bcolj = new double[n];
            for (int j = 0; j < p; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    Bcolj[k] = other[k, j];
                }

                for (int i = 0; i < m; i++)
                {
                    double s = 0;
                    for (int k = 0; k < n; k++)
                    {
                        s += original[i, k] * Bcolj[k];
                    }

                    result[i, j] = s;
                }
            }
            return result;
        }

        public static double[,] SubMatrix(this double[,] original, int startRow, int endRow, int startCol, int endCol)
        {
            double[,] newMatrix = new double[endRow - startRow, endCol - startCol];
            for (int i = startRow; i < endRow; i++)
            {
                for (int j = startCol; j < endCol; j++)
                {
                    newMatrix[i - startRow, j - startCol] = original[i, j];
                }
            }
            return newMatrix;
        }

        public static double[] GetRange(this double[] original, int startRow)
        {
            double[] returnArray = new double[original.Length - startRow];

            for(int i = startRow; i < original.Length; i++)
            {
                returnArray[i - startRow] = original[i];
            }
            return returnArray;
        }

        public static float[] AbsoluteValue(this float[] norm)
        {
            float[] absoluteArray = new float[norm.Length];
            for (int i = 0; i < norm.Length; i++)
            {
                absoluteArray[i] = Math.Abs(norm[i]);
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

        public static float Max(this float[] value)
        {
            return value.Max();
        }

        public static double[] Divide(this double[] val1, double val2)
        {
            double[] returnValue = new double[val1.Length];

            for (int i = 0; i < val1.Length; i++)
            {
                returnValue[i] = val1[i] / val2;
            }

            return returnValue;
        }

        public static T[,] Transpose<T>(this T[,] original)
        {
            T[,] newArray = new T[original.GetLength(1), original.GetLength(0)];

            for(int i = 0; i < original.GetLength(0); i++)
            {
                for(int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[j, i] = original[i, j];
                }
            }


            return newArray;
        }

        #region Unused - better than losing the work
        public static Vector3 MultiplyVector(this Vector3 vector, Quaternion rotation)
        {
            float x = rotation.X * 2f;
            float y = rotation.Y * 2f;
            float z = rotation.Z * 2f;

            float xx = rotation.X = x;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;

            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yz = rotation.Y * z;

            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;

            Vector3 newVector = new Vector3();
            newVector.X = (1f - (yy + zz)) * vector.X + (xy - wz) * vector.Y + (xz + wy) * vector.Z;
            newVector.Y = (xy + wz) * vector.X + (1F - (xx + zz)) * vector.Y + (yz - wx) * vector.Z;
            newVector.Z = (xz - wy) * vector.X + (yz + wx) * vector.Y + (1F - (xx + yy)) * vector.Z;
            return newVector;
        }

        public static Vector3 GetCol(this float[,] matrix, int column)
        {
            return new Vector3(matrix[0, column], matrix[1, column], matrix[2, column]);
        }

        public static float[,] TopRows(this float[,] original, int numRows)
        {
            float[,] item = new float[numRows, original.GetLength(1)];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    item[i, j] = original[i, j];
                }
            }

            return item;
        }

        public static void SetColumn(this float[,] matrix, int column, float[] newValue)
        {
            for (int i = 0; i < matrix.GetLength(1); i++)
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

        public static float[] GetRow(this float[,] original, int row)
        {
            float[] newArray = new float[original.GetLength(1)];

            for (int i = 0; i < original.GetLength(1); i++)
            {
                newArray[i] = original[row, i];
            }

            return newArray;
        }

        public static float StableNorm(this float[] array)
        {
            int index = array.Length;
            float norm = -1;

            if (index == 1)
            {
                norm = Math.Abs(array[0]);
            }
            else
            {
                float scale = 0;
                float inverseScale = 1;
                float ssq = 0;

                float maxCoefficient = array.AbsoluteValue().MaxCoefficient(out _);
                if (maxCoefficient > scale)
                {
                    ssq = ssq * Math.Abs(scale / maxCoefficient);
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

                norm = scale = (float)Math.Sqrt(ssq);
            }
            return norm;
        }

        public static double Abs2(this double value)
        {
            return Math.Abs(Math.Pow(value, 2));
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

        public static double Dot(this double[] original, double[] additional)
        {
            double sum = 0;
            //multiply first row by first column
            for (int i = 0; i < original.Length; i++)
            {
                sum += original[i] * additional[i];
            }

            return sum;
        }

        public static double[] Multiply(this double[] original, double[] additional)
        {
            double[] newArray = new double[original.Length];
            //multiply first row by first column
            for (int i = 0; i < original.Length; i++)
            {
                newArray[i] = original[i] * additional[i];
            }

            return newArray;
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

        public static double[] Multiply(this double[] original, double factor)
        {
            double[] newArray = new double[original.GetLength(0)];

            for (int i = 0; i < original.GetLength(0); i++)
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
            for (int i = 0; i < permutation.GetLength(0); i++)
            {
                //find indexes of the true values [i,j]
                //i = row to move original row to
                //j = row index to move
                for (int j = 0; j < permutation.GetLength(1); j++)
                {
                    if (permutation[i, j])
                    {
                        newArray[i] = original[j];
                    }
                }
            }
            return newArray;
        }

        public static float[] GetDiagonal(this float[,] original)
        {
            float[] newArray = new float[(original.GetLength(0) * original.GetLength(1)) / 2];
            int count = 0;
            //based off left multiplication - rearranges corresponding rows
            for (int i = 0; i < original.GetLength(0); i++)
            {
                //find indexes of the true values [i,j]
                //i = row to move original row to
                //j = row index to move
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    if (i == j)
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
            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    if (i == j)
                    {
                        original[i, j] = values[count];
                        count++;
                    }
                }
            }
        }

        public static float[,] Multiply(this float[,] original, float factor)
        {

            float[,] newArray = new float[original.GetLength(0), original.GetLength(1)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[i, j] = original[i, j] * factor;
                }
            }

            return newArray;
        }


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

        public static double[] Subtract(this double[] original, double[] value)
        {
            double[] newArray = new double[original.GetLength(0)];
            for (int i = 0; i < original.GetLength(0); i++)
            {
                newArray[i] = original[i] - value[i];
            }
            return newArray;
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
                    Bcolj[k] = other[k, j];
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
                    Bcolj[k] = other[k, j];
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

            for (int i = 0; i < array.Length; i++)
            {
                sum += Abs2(array[i]);
            }

            return (float)Math.Sqrt(sum);
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
                    Bcolj[k] = other[k, j];
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

        public static void SwapCol(this float[,] original, int originalColIndex, int replacementColIndex)
        {
            var temp = original.GetColumn(originalColIndex);

            for (int i = 0; i < original.GetLength(0); i++)
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

        public static float Norm(this float[] original)
        {
            return original.Sum();
        }

        public static float[,] UpperTrianglarView(this float[,] original)
        {
            float[,] newArray = new float[original.GetLength(0), original.GetLength(1)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
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
            for (int i = 0; i < val.Length; i++)
            {
                returnVal[i] = Math.Max(val[i], val2[i]);
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

            for (int i = 0; i < initial.Length; i++)
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

        public static double[,] EnsureZerosLowerTriangle(this double[,] A, int numRows, int numCols)
        {
            double[,] returnArray = new double[numRows, numCols];
            returnArray.Fill(0);
            //if A is null or incorrect size then return a full array of 0s
            if (A != null && numRows == A.GetLength(0) && numCols == A.GetLength(1))
            {
                //fill only lower triangle with 0s
                for (int i = 0; i < A.GetLength(0); i++)
                {
                    for (int j = 0; j < A.GetLength(1); j++)
                    {
                        if (i < j)
                        {
                            A[i, j] = 0;
                        }
                    }
                }
            }

            return returnArray;
        }

        public static double FindMax(this double[] u, int startIndex, int length)
        {
            double max = -1;

            int stopIndex = startIndex + length;
            for (int index = startIndex; index < stopIndex; index++)
            {
                double val = u[index];
                val = (val < 0.0) ? -val : val;
                if (val > max)
                    max = val;
            }

            return max;
        }

        #endregion
    }
}

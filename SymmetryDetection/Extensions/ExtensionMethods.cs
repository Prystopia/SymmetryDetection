using Microsoft.Win32.SafeHandles;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Interfaces;
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

        public static float[] GetColumn(this float[,] matrix, int column)
        {
            float[] val = new float[matrix.GetLongLength(column)];
            for (int i = 0; i < matrix.GetLongLength(column); i++)
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

        public static double[] GetRow(this double[,] original, int row)
        {
            double[] newArray = new double[original.GetLength(1)];

            for (int i = 0; i < original.GetLength(1); i++)
            {
                newArray[i] = original[row, i];
            }

            return newArray;
        }

        public static float Magnitude(this Vector3 original)
        {
            return MathF.Sqrt(MathF.Pow(original.X, 2) + MathF.Pow(original.Y, 2) + MathF.Pow(original.Z, 2));
        }

        public static double SquaredNorm(this double[] original)
        {
            return Math.Sqrt(original.Select(x => Math.Pow(x, 2)).Sum());
        }


        public static float Abs2(this float value)
        {
            return MathF.Abs(MathF.Pow(value, 2));
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

        public static double[] Divide(this double[] val1, double val2)
        {
            double[] returnValue = new double[val1.Length];

            for (int i = 0; i < val1.Length; i++)
            {
                returnValue[i] = val1[i] / val2;
            }

            return returnValue;
        }

        public static double[,] Transpose(this double[,] original)
        {
            double[,] newArray = new double[original.GetLength(1), original.GetLength(0)];

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    newArray[j, i] = original[i, j];
                }
            }


            return newArray;
        }
    }
}

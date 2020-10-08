using SymmetryDetection.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace SymmetryDetection.Test
{
    public class ExtensionMethodTests
    {
        /*public static Vector3 Multiply(this float[,] original, Vector3 factor)
        {
            if(original.GetLength(1) != 3)
            {
                throw new ArgumentException();
            }
            Vector3 newVector = new Vector3();
            newVector.X = factor.X * original[0, 0] + factor.Y * original[0, 1] + factor.Y * original[0, 2];
            newVector.Y = factor.X * original[1, 0] + factor.Y * original[1, 1] + factor.Y * original[1, 2];
            newVector.Z = factor.X * original[2, 0] + factor.Y * original[2, 1] + factor.Y * original[2, 2];
            return newVector;
        }
*/
        [Fact]
        public void Multiply_Matrix_By_Vector_Test_1()
        {
            float[,] matrix = new float[3, 3]
            {
                { 1, 1, 1 },
                { 2, 2, 2 },
                { 3, 3, 3 },
            };
            Vector3 vector = new Vector3(1, 2, 3);
            Vector3 result = ExtensionMethods.Multiply(matrix, vector);

            Assert.Equal(6, result.X);
            Assert.Equal(12, result.Y);
            Assert.Equal(18, result.Z);
        }

        [Fact]
        public void Multiply_Matrix_By_Vector_Test_2()
        {
            float[,] matrix = new float[3, 3]
            {
                { 1, -1, 2 },
                { 0, -3, 1 },
                { 0, 0, 0 },
            };
            Vector3 vector = new Vector3(2, 1, 0);
            Vector3 result = ExtensionMethods.Multiply(matrix, vector);

            Assert.Equal(1, result.X);
            Assert.Equal(-3, result.Y);
            Assert.Equal(0, result.Z);
        }
    }
}

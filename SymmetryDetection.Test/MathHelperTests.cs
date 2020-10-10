using SymmetryDetection.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SymmetryDetection.Test
{
    public class MathHelperTests
    {
        [Fact]
        public void Median_Returns_Middle_Value_In_Odd_Collection()
        {
            var array = new float[]{
                1, 2, 3, 4, 5
            };

            var result = MathsHelpers.Median(array);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Median_Returns_Averaged_Value_In_Even_Collection()
        {
            var array = new float[]{
                1, 2, 3, 4
            };

            var result = MathsHelpers.Median(array);

            Assert.Equal(2.5, result);

        }
        [Fact]
        public void Median_Returns_Averaged_Value_In_Large_Even_Collection()
        {
            var array = new float[]{
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };

            var result = MathsHelpers.Median(array);

            Assert.Equal(5.5, result);
        }

            [Fact]
        public void Median_Returns_Orders_Array()
        {
            var array = new float[]{
                3, 4, 1, 2
            };

            var result = MathsHelpers.Median(array);

            Assert.Equal(2.5, result);
        }
    }
}

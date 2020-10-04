using System;
using SymmetryDetection.Optimisation;
using SymmetryDetection.Refinement;
using Xunit;

namespace SymmetryDetection.Test
{

    class TestFunctor : LMFunction
    {
        public int InputSize => 2;

        public int ValuesSize => 7;

        private float[] vals = new float[] { 0.03f, 0.1947f, 0.425f, 0.626f, 1.253f, 2.5f, 3.74f };

        public float[] Function(float[] input)
        {
            var errors = new float[vals.Length];

            for (int i = 0; i < vals.Length; i++)
            {
                errors[i] = (input[0] * vals[i]) / (input[1] + vals[i]);
            }

            return errors;
        }
    }
    public class LevenbergMarquardtTests
    {
        [Fact]
        public void Example_Test_From_Accord_Net()
        {
            //Not sure this is a good test as im not sure what these outputs are used for
            //float[] outputs = new float[] { 0.05f, 0.127f, 0.094f, 0.2122f, 0.2729f, 0.2665f, 0.3317f };

            TestFunctor function = new TestFunctor();
            LevenbergMarquadtEJML lm = new LevenbergMarquadtEJML(function);
            var solution = lm.Minimise(new float[] { 0.9f, 0.2f });

            Assert.Equal(2, solution.Length);
            //Assert.Equal(0.362f, solution[0]);
            //Assert.Equal(0.556f, solution[1]);

        }
    }
}

using SymmetryDetection.Interfaces;
using SymmetryDetection.Optimisation;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SymmetryDetection.Test
{
    public class LevenbergMarquardtTests
    {
        private class TestFunctor : IOptimisationFunction
        {
            public int InputSize => 2;

            public int ValuesSize => 2;

            private List<int> Values = new List<int>()
            {
                1, 2
            };

            public double[] Function(double[] input)
            {
                //this is the error function
                double[] errors = new double[ValuesSize];

                errors[0] = Math.Abs(Values[0] - input[0]);
                errors[1] = Math.Abs(Values[1] - input[1]);

                return errors;

            }
        }
        private LevenbergMarquadtEJML Service { get; set; }

        public LevenbergMarquardtTests()
        {
            this.Setup();
        }

        private void Setup()
        {
            IDecompositionHandler decompositionHandler = new AlglibQRDecomposer();
            ILinearSolver linearSolver = new LinearSolverHouseHolder(decompositionHandler);
            //set the function tolerence to 0 to ensure we get exact answers
            this.Service = new LevenbergMarquadtEJML(new TestFunctor(), linearSolver);
        }

        [Fact]
        public void Minimise_Reduces_Input_Parameters()
        {
            double[] input = new double[]
            {
                0.1, 0.1
            };
            var solution = this.Service.Minimise(input);

            Assert.NotNull(solution);
            Assert.Equal(2, solution.Length);
            //have to round the answers as they will only be approximate (but very close)
            Assert.Equal(1, Math.Round(solution[0], 2));
            Assert.Equal(2, Math.Round(solution[1], 2));
        }
    }
}

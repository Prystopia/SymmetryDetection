using SymmetryDetection.Extensions;
using SymmetryDetection.Refinement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SymmetryDetection.Optimisation
{
    /// <summary>
    /// LM implementation taken from https://github.com/mljs/levenberg-marquardt
    /// </summary>
    public class LevenbergMarquardtMLJS
    {
        public int MaxIterations { get; set; }
        public float gradientDifference => 10e-2f;
        public float ErrorTolerance => 10e-2f;
        public float Damping { get; set; }
        //private float[] MinValues { get; set; }
        //private float[] MaxValues { get; set; }
        private LMFunction Functor { get; set; }

        public LevenbergMarquardtMLJS(LMFunction functor)
        {
            this.MaxIterations = 100;
            this.Damping = 1;
            this.Functor = functor;
        }

        public float[] Run(float[] parameters) //input = data
        {
            if(Damping <= 0)
            {
                throw new Exception("The damping option must be a positive number");
            }

            //if (values == null || values.Length < 2)
            //{
            //    throw new Exception("The values parameter elements must be an array with more than 2 points");
            //}
            //if (input.Length != values.Length)
            //{
            //    throw new Exception("The data parameter elements must have the same size");
            //}

            //MaxValues = new float[Functor.InputSize];
            //Array.Fill(MaxValues, float.MaxValue);
            //MinValues = new float[Functor.InputSize];
            //Array.Fill(MinValues, float.MinValue);

            float currentError = Functor.Function(parameters).Sum();

            bool converged = currentError <= ErrorTolerance;

            for(int iter = 0; iter < MaxIterations && !converged; iter++)
            {
                parameters = Step(parameters);

                //Max and min values is never set so do not need this
                //for (int k = 0; k < parameterLength; k++)
                //{
                //    parameters[k] = MathF.Min(MathF.Max(MinValues[k], parameters[k]), MaxValues[k]);
                //}

                //check for convergence
                currentError = Functor.Function(parameters).Sum();
                if(float.IsNaN(currentError))
                {
                    break;
                }
                converged = currentError <= ErrorTolerance;
            }
            return parameters;
        }
        public float[][] GradientFunction(float[] evaluatedData, float[] parameters)
        {
            int n = parameters.Length;
            int m = Functor.ValuesSize;

            float[][] ans = new float[n][];

            for (int i = 0; i < n; i++)
            {
                ans[i] = new float[m];
                var auxParams = (float[])parameters.Clone();
                auxParams[i] += gradientDifference;

                for (int point = 0; point < m; point++)
                {
                    ans[i][point] = evaluatedData[point] - Functor.Function(auxParams)[point];
                }
            }
            return ans;
        }

        public float[][] MatrixFunction(float[] evaluatedData, float[] parameters)
        {
            int m = 1;// parameters.Length;

            //A*X = B

            float[][] ans = new float[m][];

            for (int point = 0; point < m; point++)
            {
                ans[point] = new float[] { parameters[point] - evaluatedData[point] };
            }

            return ans;
        }

        public float[] Step(float[] parameters)
        {
            float value = Damping * MathF.Pow(gradientDifference, 2); // lamda in some implementations
            float[,] identity = new float[parameters.Length, parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                identity[i, i] = value;
            }

            float[] evaluatedData = Functor.Function(parameters);
            float[][] gradient = GradientFunction(evaluatedData, parameters);
            float[][] matrix = MatrixFunction(parameters, evaluatedData);//TODO - Check if this is correct

            var gradientMultipliedTransposed = gradient.MatrixMultiply(gradient.Transpose());
            var identityCopy = (float[,])identity.Clone();
            for (int i = 0; i < identityCopy.GetLength(0); i++)
            {
                for (int j = 0; j < identityCopy.GetLength(1); j++)
                {
                    identityCopy[i, j] = identityCopy[i, j] + gradientMultipliedTransposed[i][j];
                }
            }

            float[,] inverseMatrix = Inverse(identityCopy);


            //var negativeStep = gradient.MatrixMultiply(inverseMatrix);//I.E. Solve
            //subtract negative step from current parameter values

            float[,] parametersCopy = new float[1, parameters.Length];
            for(int i = 0; i < parameters.Length; i++)
            {
                parametersCopy[0, i] = parameters[i];
            }

            var newParams = parametersCopy.Sub(
                                    inverseMatrix.MatrixMultiply(gradient).MatrixMultiply(matrix).Multiply(gradientDifference).Transpose()//Solve  = inverse matrix code A*X = B, A = IdentityMaxtrix, B == Gradient therefore X = B / A
                                  );

            return newParams.To1DArray();
        }
        private float[,] Inverse(float[,] leftHand)
        {
            var rightHand = new float[leftHand.GetLength(0), leftHand.GetLength(0)];
            for (int i = 0; i < leftHand.GetLength(0); i++)
            {
                for(int j = 0; j < leftHand.GetLength(0); j++)
                {
                    rightHand[i, j] = 1;
                }
            }
            return new QRDecomposition(leftHand).Solve(rightHand);
        }
       
    }
}


using System;
using System.Collections.Generic;
using System.Text;
using SymmetryDetection.Extensions;
using SymmetryDetection.Refinement;

namespace SymmetryDetection.Optimisation
{
    public class LevenbergMarquadtEJML
    {
        //GradientTolerance - Gtol
        //FunctionTolerance - Ftol
        //https://github.com/lessthanoptimal/ejml/blob/v0.35/examples/src/org/ejml/example/LevenbergMarquardt.java
        //gradient is the jacobean and the residuals and the errors
        //identity is the jacobean and jacobean
        //hdiag is the diagonal of the identity matrix

        public int MaxIterations { get; set; }
        public float FunctionTolerance => 1e-12f;
        public float GradiantTolerance => 1e-12f;
        public float Delta => 1e-7f;

        public float InitialLambda { get; set; }
        public LMFunction Functor { get; set; }
        public  float InitialCost { get; set; }
        public float FinalCost { get; set; }

        private float[,] Gradient { get; set; }
        private float[,] HessianApprox { get; set; }
        private float[] HessianDiagonal { get; set; }
        private float[,] NegativeStep { get; set; }
        private float[] Temp0 { get; set; }
        private float[] Temp1 { get; set; }
        private float[,] Residuals { get; set; }
        private float[,] Jacobian { get; set; }

        public LevenbergMarquadtEJML(LMFunction functor, float initialLambda = 1)
        {
            this.MaxIterations = 100;
            this.Functor = functor;
            this.InitialLambda = initialLambda;
            this.Configure();
        }

        public float[] Minimise(float[] parameters)
        {
            float previousCost = InitialCost = CalculateError(parameters);

            float lambda = InitialLambda;
            //determines whether we should re-compute the jacobian matrix;
            bool computeHessian = true;

            for(int iter = 0; iter < MaxIterations; iter++)
            {
                if(computeHessian)
                {
                    ComputeGradientAndHessian(parameters);
                    computeHessian = false;

                    bool converged = true;
                    for(int i = 0; i < Functor.InputSize; i++)
                    {
                        if(MathF.Abs(Gradient[i, 0]) > GradiantTolerance)
                        {
                            converged = false;
                            break;
                        }
                    }
                    if(converged)
                    {
                        return parameters;
                    }
                }

                for(int i = 0; i < HessianApprox.GetLength(0); i++)
                {
                    HessianApprox[i, i] = HessianDiagonal[i] + lambda;
                }

                 Solve();

                var parameterCopy = new float[parameters.Length, 1];
                for(int i = 0; i < parameters.Length; i++)
                {
                    parameterCopy[i, 0] = parameters[i];
                }

                var candidateParams = parameterCopy.Sub(NegativeStep);

                float cost = CalculateError(candidateParams.To1DArray());

                if(cost <= previousCost)
                {
                    computeHessian = true;
                    parameters = candidateParams.To1DArray();

                    bool converged = FunctionTolerance * previousCost >= previousCost - cost;

                    previousCost = cost;
                    lambda /= 10;
                    if(converged)
                    {
                        return parameters;
                    }
                }
                else
                {
                    lambda *= 10;
                }
            }
            FinalCost = previousCost;
            return parameters;
        }

        private void Configure()
        {
            Gradient = new float[Functor.InputSize, 1];
            HessianApprox = new float[Functor.InputSize, Functor.InputSize];

            NegativeStep = new float[Functor.InputSize, 1];

            Temp0 = new float[Functor.ValuesSize];
            Temp1 = new float[Functor.ValuesSize];
            Residuals = new float[Functor.ValuesSize, 1];
            Jacobian = new float[Functor.ValuesSize, Functor.InputSize];
            HessianDiagonal = new float[Functor.InputSize];
        }

        /// <summary>
        /// Calculates the cost of the parameters given 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private float CalculateError(float[] parameters)
        {
            var errorArray = Functor.Function(parameters);

            float error = errorArray.FrobiusNorm();

            //ensure the values are copied correctly
            for (int i = 0; i < errorArray.Length; i++)
            {
                Residuals[i, 0] = errorArray[i];
            }

            return error * error / Residuals.GetLength(0);
        }
        private void Solve()
        {
            //A = HessianApprox, b = Gradient, X = NegativeStep;

            //A*x = b
            //X = B/A = Inverse(A) * B
            NegativeStep = Inverse(HessianApprox).MatrixMultiply(Gradient);
        }
        private float[,] Inverse(float[,] leftHand)
        {
            var rightHand = new float[leftHand.GetLength(0), leftHand.GetLength(0)];
            for (int i = 0; i < leftHand.GetLength(0); i++)
            {
                for (int j = 0; j < leftHand.GetLength(0); j++)
                {
                    rightHand[i, j] = 1;
                }
            }
            return new QRDecomposition(leftHand).Solve(rightHand);
        }
        private void ComputeGradientAndHessian(float[] parameters)
        {
            var errors = Functor.Function(parameters);

            for (int i = 0; i < errors.Length; i++)
            {
                Residuals[i, 0] = errors[i];
            }

            ComputeNumericalJacobian(parameters);
            Gradient = Jacobian.Transpose().MatrixMultiply(Residuals);
            //Need to check this
            HessianApprox = Jacobian.Transpose().MatrixMultiply(Jacobian);
            ExtractHessianDiagonal();
        }

        private void ComputeNumericalJacobian(float[] parameters)
        {
            float invDelta = 1 / Delta;

            Temp0 = Functor.Function(parameters);

            for(int i = 0; i < Functor.InputSize; i++)
            {
                parameters[i] = parameters[i] + Delta;
                Temp1 = Functor.Function(parameters);
                Temp1 = ComputeDifference(invDelta, Temp1, -invDelta, Temp0);

                AddToJacobianArray(Temp1, 0, i);

                parameters[i] -= Delta;
            }

        }

        private float[] ComputeDifference(float alpha, float[] val1, float beta, float[] val2)
        {
            //c = α * a + β * b
            //cij = α * aij + β * bij
            float[] newArray = new float[val1.Length];
            for(int i = 0; i < val1.Length; i++)
            {
                newArray[i] = (alpha * val1[i]) + (beta * val2[i]);
            }

            return newArray;
        }

        private void AddToJacobianArray(float[] src, int row, int col)
        {
            //Inserts matrix 'src' into matrix 'dest' with the(0,0) of src at(row, col) in dest.
            for(int i = 0; i < src.Length; i++)
            {
                Jacobian[i, col] = src[i];
            }
        }

        private void ExtractHessianDiagonal()
        {
            for(int i = 0; i < HessianApprox.GetLength(0); i++)
            {
                HessianDiagonal[i] = HessianApprox[i, i];
            }
        }
    }
}

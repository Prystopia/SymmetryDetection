using System;
using System.Collections.Generic;
using System.Text;
using SymmetryDetection.Extensions;
using SymmetryDetection.Interfaces;
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
        public double FunctionTolerance => 1e-12f;
        public double GradiantTolerance => 1e-12f;
        public double Delta => 1e-8f;

        public double InitialLambda { get; set; }
        public LMFunction Functor { get; set; }
        public double InitialCost { get; set; }
        public double FinalCost { get; set; }

        private double[,] Gradient { get; set; }
        private double[,] HessianApprox { get; set; }
        private double[] HessianDiagonal { get; set; }
        private double[,] NegativeStep { get; set; }
        private double[] Temp0 { get; set; }
        private double[] Temp1 { get; set; }
        private double[,] Residuals { get; set; }
        private double[,] Jacobian { get; set; }

        private ILinearSolver Solver { get; set; }

        public LevenbergMarquadtEJML(LMFunction functor, ILinearSolver solver, double initialLambda = 1)
        {
            this.MaxIterations = 400;
            this.Functor = functor;
            this.InitialLambda = initialLambda;
            this.Solver = solver;
            this.Configure();
        }

        public double[] Minimise(double[] parameters)
        {
            double previousCost = InitialCost = CalculateError(parameters);

            double lambda = InitialLambda;
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
                        if(Math.Abs(Gradient[i, 0]) > GradiantTolerance)
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

                if (!Solve())
                {
                    return parameters;
                }

                var parameterCopy = new double[parameters.Length, 1];
                for(int i = 0; i < parameters.Length; i++)
                {
                    parameterCopy[i, 0] = parameters[i];
                }

                var candidateParams = parameterCopy.Subtract(NegativeStep);

                double cost = CalculateError(candidateParams.To1DArray());

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
            Gradient = new double[Functor.InputSize, 1];
            HessianApprox = new double[Functor.InputSize, Functor.InputSize];

            NegativeStep = new double[Functor.InputSize, 1];

            Temp0 = new double[Functor.ValuesSize];
            Temp1 = new double[Functor.ValuesSize];
            Residuals = new double[Functor.ValuesSize, 1];
            Jacobian = new double[Functor.ValuesSize, Functor.InputSize];
            HessianDiagonal = new double[Functor.InputSize];
        }

        /// <summary>
        /// Calculates the cost of the parameters given 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private double CalculateError(double[] parameters)
        {
            var errorArray = Functor.Function(parameters);

            double error = errorArray.SquaredNorm();

            //ensure the values are copied correctly
            for (int i = 0; i < errorArray.Length; i++)
            {
                Residuals[i, 0] = errorArray[i];
            }

            return error * error / Residuals.GetLength(0);
        }
        public bool Solve()
        {
            if (!Solver.SetA(HessianApprox))
                return false;

            NegativeStep = Solver.Solve(Gradient);
            return true;
        }
        
        private void ComputeGradientAndHessian(double[] parameters)
        {
            var errors = Functor.Function(parameters);

            for (int i = 0; i < errors.Length; i++)
            {
                Residuals[i, 0] = errors[i];
            }

            ComputeNumericalJacobian(parameters);
            Gradient = Jacobian.Transpose().Multiply(Residuals);
            //Need to check this
            HessianApprox = Jacobian.Transpose().Multiply(Jacobian);
            ExtractHessianDiagonal();
        }

        private void ComputeNumericalJacobian(double[] parameters)
        {
            double invDelta = 1 / Delta;

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

        private double[] ComputeDifference(double alpha, double[] val1, double beta, double[] val2)
        {
            //c = α * a + β * b
            //cij = α * aij + β * bij
            double[] newArray = new double[val1.Length];
            for(int i = 0; i < val1.Length; i++)
            {
                newArray[i] = (alpha * val1[i]) + (beta * val2[i]);
            }

            return newArray;
        }

        private void AddToJacobianArray(double[] src, int row, int col)
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

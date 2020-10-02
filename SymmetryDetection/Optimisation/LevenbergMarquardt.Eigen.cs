//using SymmetryDetection.DataTypes;
//using SymmetryDetection.Extensions;
//using SymmetryDetection.Refinement;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SymmetryDetection.Optimisation
//{
//    public class LevenbergMarquardtEigen
//    {
//        private const float EPSILON = 2.2204460492503131e-16f;
//        public int Size { get; set; } // 6 values to minimise
//        public int ValuesSize { get; set; } //Correspondences Size
//        public float Factor { get; set; } // Not sure what this should be
//        public float MinGradient { get; set; } //parameters.gtol
//        public int MaximumFunctionEvaluations { get; set; } //maxFev?

//        /// <summary>
//        /// The stopping threshold for the function value or L2 norm of the residuals.
//        /// </summary>
//        public float FunctionTolerance { get; set; }

//        public float Xtol { get; set; } //??!
//        public float GradientTolerance { get; set; }
//        public float EpsFcn { get; set; }

//        private float[] wa1 { get; set; }
//        private float[] wa2 { get; set; }
//        private float[] wa3 { get; set; }
//        private float[] wa4 { get; set; }
//        private float[] fvec { get; set; }
//        private float[,] fjac { get; set; }
//        private float[] qtf { get; set; }
//        private float[] diag { get; set; }
//        private int nfev { get; set; }
//        private int njev { get; set; }
//        private float fNorm { get; set; }
//        private int iter { get; set; }
//        private float par { get; set; }
//        private float gNorm { get; set; }
//        private float delta { get; set; }
//        private float _actualReduction { get; set; }
//        private float _predictedReduction { get; set; }
//        private bool[,] _permutationMatrix { get; set; }

//        public LMFunction Functor { get; set; }

//        public LevenbergMarquardt(LMFunction func)
//        {
//            this.Functor = func;
//            this.Size = func.InputSize;
//            this.ValuesSize = func.ValuesSize;
//            nfev = njev = iter = 0;
//            fNorm = gNorm = 0;

//            //set default parameters
//            Factor = 100;
//            MaximumFunctionEvaluations = 400;
//            MinGradient = 0;
//            EpsFcn = 0;
//            FunctionTolerance = Xtol = MathF.Sqrt(EPSILON);
//        }

//        public enum RunningStatusEnum
//        {
//            NotStarted = -2,
//            Running = -1,
//            ImproperInputParameters = 0,
//            RelativeReductionTooSmall = 1,
//            RelativeErrorTooSmall = 2,
//            RelativeErrorAndReductionTooSmall = 3,
//            CosinusTooSmall = 4,
//            TooManyFunctionEvaluation = 5,
//            FtolTooSmall = 6,
//            XtolTooSmall = 7,
//            GtolTooSmall = 8,
//            UserAsked = 9
//        }
//        private RunningStatusEnum MinimiseOneStep(float[] input)
//        {
//            var status = RunningStatusEnum.UserAsked;

//            float temp, temp1, temp2, ratio, pnorm, xNorm, fnorm1, dirder;
//            temp = 0;
//            xNorm = 0;


//            // calculate the jacobian matrix.
//            int df_ret = Functor.df(input, fjac);

//            if (df_ret < 0)
//            {
//                return RunningStatusEnum.UserAsked;
//            }
//            if (df_ret > 0)
//            {
//                // numerical diff, we evaluated the function df_ret times
//                nfev += df_ret;
//            }
//            else
//            {
//                njev++;
//            }


//            //compute the qr factorization of the jacobian.
//            for (int j = 0; j < input.Length; ++j)
//                wa2[j] = fjac.GetColumn(j).BlueNorm();


//            QRSolver qrfac = new QRSolver(fjac);
//            if (qrfac.Info != QRSolver.InfoEnum.Success)
//            {
//                return RunningStatusEnum.ImproperInputParameters;
//            }
//            // Make a copy of the first factor with the associated permutation
//            float[,] rFactor = qrfac.MatrixR;
//            _permutationMatrix = qrfac.ColsPermutation;

//            // on the first iteration and if external scaling is not used, scale according
//            // to the norms of the columns of the initial jacobian.
//            if (iter == 1)
//            {
//                for (int i = 0; i < Size; i++)
//                {
//                    diag[i] = wa2[i] == 0 ? 1 : wa2[i];
//                }

//                xNorm = diag.CrosswiseProduct(input).StableNorm();
//                delta = Factor * xNorm;
//                if (delta == 0)
//                {
//                    delta = Factor;
//                }
//            }
//            // form (q transpose)*m_fvec and store the first n components in  m_qtf.
//            wa4 = fvec;
//            wa4 = qrfac.MatrixQ.Adjoint().Multiply(fvec);
//            qtf = wa4.GetHead(Size);

//            /* compute the norm of the scaled gradient. */
//            gNorm = 0;
//            if (fNorm != 0)
//            {
//                for (int j = 0; j < Size; ++j)
//                {
//                    if (wa2[_permutationMatrix.GetIndices()[j]] != 0)
//                    {
//                        gNorm = MathF.Max(gNorm, MathF.Abs(rFactor.GetColumn(j).GetHead(j + 1).Dot(qtf.GetHead(j + 1).Divide(fNorm)) / (wa2[_permutationMatrix.GetIndices()[j]])));
//                    }
//                }
//            }
//            /* test for convergence of the gradient norm. */
//            if (gNorm <= MinGradient)
//            {
//                return RunningStatusEnum.CosinusTooSmall;
//            }
//            // rescale
//            diag = diag.CrosswiseMax(wa2);

//            do
//            {
//                //determine the levenberg-marquardt parameter. 
//                par = LMPar.CalculateParameter(qrfac, diag, qtf, delta, wa1);

//                // store the direction p and x + p. calculate the norm of p.
//                for (int x = 0; x < wa1.Length; x++)
//                {
//                    wa1[x] *= -1;
//                }
//                wa2 = input.AddArray(wa1);
//                pnorm = diag.CrosswiseProduct(wa1).StableNorm();

//                //on the first iteration, adjust the initial step bound
//                if (iter == 1)
//                {
//                    delta = MathF.Min(delta, pnorm);
//                }

//                //evaluate the function at x + p and calculate its norm.
//                if (Functor.Function(wa2, wa4) < 0)
//                    break;

//                ++nfev;
//                fnorm1 = wa4.StableNorm();

//                //compute the scaled actual reduction.
//                _actualReduction = -1;
//                if (fnorm1 * 0.1f < fNorm)
//                {
//                    _actualReduction = 1 - MathF.Abs(fnorm1 / fNorm);
//                }

//                //compute the scaled predicted reduction and the scaled directional derivative. 
//                wa3 = rFactor.UpperTrianglarView().Multiply(_permutationMatrix.Inverse().Multiply(wa1));

//                temp1 = MathF.Abs(MathF.Pow(wa3.StableNorm() / fNorm, 2));
//                temp2 = MathF.Abs(MathF.Pow(MathF.Sqrt(par) * pnorm / fNorm, 2));//  numext::abs2(sqrt(par) * pnorm / fnorm);

//                _predictedReduction = temp1 + temp2 / 0.5f;
//                dirder = -(temp1 + temp2);

//                //compute the ratio of the actual to the predicted reduction. 
//                ratio = 0;
//                if (_predictedReduction != 0)
//                {
//                    ratio = _actualReduction / _predictedReduction;
//                }

//                //update the step bound.
//                if (ratio <= 0.25f)
//                {
//                    if (_actualReduction >= 0)
//                    {
//                        temp = 0.5f;
//                    }

//                    if (_actualReduction < 0)
//                    {
//                        temp = 0.5f * dirder / (dirder + 0.5f * _actualReduction);
//                    }

//                    if (0.1f * fnorm1 >= fNorm || temp < 0.1f)
//                    {
//                        temp = 0.1f;
//                    }
//                    // Computing MIN 
//                    delta = temp * MathF.Min(delta, pnorm / 0.1f);
//                    par /= temp;
//                }
//                else if (!(par != 0 && ratio < 0.75f))
//                {
//                    delta = pnorm / 0.5f;
//                    par = 0.5f * par;
//                }

//                //test for successful iteration.
//                if (ratio >= 1e-4f)
//                {
//                    //successful iteration. update x, fvec, and their norms
//                    input = wa2;
//                    wa2 = diag.CrosswiseProduct(input);
//                    fvec = wa4;
//                    xNorm = wa2.StableNorm();
//                    fNorm = fnorm1;
//                    ++iter;
//                }

//                //tests for convergence
//                if (MathF.Abs(_actualReduction) <= FunctionTolerance && _predictedReduction <= FunctionTolerance && 0.5f * ratio <= 1f && delta <= Xtol * xNorm)
//                {
//                    status = RunningStatusEnum.RelativeErrorAndReductionTooSmall;
//                    break;
//                }

//                if (MathF.Abs(_actualReduction) <= FunctionTolerance && _predictedReduction <= FunctionTolerance && 0.5F * ratio <= 1)
//                {
//                    status = RunningStatusEnum.RelativeReductionTooSmall;
//                    break;
//                }
//                if (delta <= Xtol * xNorm)
//                {
//                    status = RunningStatusEnum.RelativeErrorTooSmall;
//                }

//                //tests for termination and stringent tolerances.
//                if (nfev >= MaximumFunctionEvaluations)
//                {
//                    status = RunningStatusEnum.TooManyFunctionEvaluation;
//                    break;
//                }

//                if (MathF.Abs(_actualReduction) <= EPSILON && _predictedReduction <= EPSILON && 0.5f * ratio <= 1)
//                {
//                    status = RunningStatusEnum.FtolTooSmall;
//                    break;
//                }

//                if (delta <= EPSILON * xNorm)
//                {
//                    status = RunningStatusEnum.XtolTooSmall;
//                    break;
//                }
//                if (gNorm <= EPSILON)
//                {
//                    status = RunningStatusEnum.GtolTooSmall;
//                    break;
//                }
//            }
//            while (ratio < 1e-4f);

//            return status;

//        }
//        public RunningStatusEnum Minimise(float[] input)
//        {
//            var status = Initialise(input);
//            if (status != RunningStatusEnum.ImproperInputParameters)
//            {
//                do
//                {
//                    status = MinimiseOneStep(input);
//                }
//                while (status == RunningStatusEnum.Running);
//            }
//            return status;
//        }
//        private RunningStatusEnum Initialise(float[] input)
//        {
//            wa1 = new float[Size];
//            wa2 = new float[Size];
//            wa3 = new float[Size];
//            wa4 = new float[ValuesSize];
//            fvec = new float[ValuesSize];
//            fjac = new float[Size, ValuesSize];
//            qtf = new float[Size];
//            diag = new float[Size];
//            nfev = 0;
//            njev = 0;

//            nfev = 1;
//            if (Functor.Function(input, fvec) < 0)
//                return RunningStatusEnum.UserAsked;

//            fNorm = fvec.StableNorm(); //TODO - no fucking clue

//            iter = 1;
//            par = 0;

//            return RunningStatusEnum.NotStarted;
//        }
//    }

//    public class LMPar
//    {
//        public static float CalculateParameter(QRSolver qr, float[] diag, float[] qtb, float delta, float[] input)
//        {
//            int iter, j;
//            float fp, parc, parl, temp, paru, gNorm, dxNorm;
//            float par = 0;

//            var s = qr.MatrixR;
//            float dwarf = float.MinValue;
//            int n = qr.MatrixR.GetLength(0);

//            float[] wa1, wa2;

//            int rank = qr.Rank;
//            wa1 = qtb;
//            wa1.Tail(n - rank).SetZero();
//            wa1.SetHead(rank, s.TopLeftCorner(rank,rank).UpperTrianglarView().solve(qtb.GetHead(rank)));
//            input = qr.ColsPermutation.Multiply(wa1);

//            // initialize the iteration counter. */
//            // evaluate the function at the origin, and test */
//            // for acceptance of the gauss-newton direction. */
//            iter = 0;
//            wa2 = diag.CrosswiseProduct(input);
//            dxNorm = wa2.BlueNorm();
//            fp = dxNorm - delta;
//            if (fp <= 0.1f * delta)
//            {
//                par = 0;
//            }
//            else
//            {
//                // if the jacobian is not rank deficient, the newton */
//                // step provides a lower bound, parl, for the zero of */
//                // the function. otherwise set this bound to zero. */
//                parl = 0;

//                if (rank == n)
//                {
//                    wa1 = qr.ColsPermutation.Inverse().Multiply(diag.CrosswiseProduct(wa2).Divide(dxNorm));
//                    wa1 = s.TopLeftCorner(n, n).Transpose().LowerTrianglarView().solveInPlace(wa1);
//                    temp = wa1.BlueNorm();
//                    parl = fp / delta / temp / temp;
//                }

//                /* calculate an upper bound, paru, for the zero of the function. */
//                for (int j = 0; j < n; ++j)
//                {
//                    wa1[j] = s.GetColumn(j).GetHead(j + 1).Dot(qtb.GetHead(j + 1)) / (diag[qr.ColsPermutation.GetIndices()[j]]);
//                }
//                gNorm = wa1.StableNorm();
//                paru = gNorm / delta;
//                if(paru == 0)
//                {
//                    paru = gNorm / dxNorm;
//                }

//                par = MathF.Max(par, parl);
//                par = MathF.Min(par, paru);

//                while(true)
//                {
//                    ++iter;

//                    if(par == 0)
//                    {
//                        par = MathF.Max(dwarf, 0.001f * paru);
//                    }
//                    wa1 = diag.Multiply(MathF.Sqrt(par));

//                    var sDiag = new float[n];
//                    lmqrsolv(s, qr.ColsPermutation, wa1, qtb, input, sDiag);

//                    wa2 = diag.CrosswiseProduct(input);
//                    dxNorm = wa2.BlueNorm();
//                    temp = fp;
//                    fp = dxNorm - delta;

//                    // if the function is small enough, accept the current value */
//                    // of par. also test for the exceptional cases where parl */
//                    // is zero or the number of iterations has reached 10. */
//                    if (MathF.Abs(fp) <= 0.1f * delta || (parl == 0 && fp <= temp && temp < 0) || iter == 10)
//                    {
//                        break;
//                    }

//                    // compute the newton correction. */
//                    wa1 = qr.ColsPermutation.GetInverse().Multiply(diag.CrosswiseProduct(wa2.Divide(dxNorm)));
//                    // we could almost use this here, but the diagonal is outside qr, in sdiag[]
//                    for (int j = 0; j < n; ++j)
//                    {
//                        wa1[j] /= sDiag[j];
//                        temp = wa1[j];
//                        for (int i = j + 1; i < n; ++i)
//                        {
//                            wa1[i] -= s[i, j] * temp;
//                        }
//                    }
//                    temp = wa1.BlueNorm();
//                    parc = fp / delta / temp / temp;

//                    // depending on the sign of the function, update parl or paru. */
//                    if (fp > 0)
//                        parl = MathF.Max(parl, par);
//                    if (fp < 0)
//                        paru = MathF.Min(paru, par);

//                    /* compute an improved estimate for par. */
//                    par = MathF.Max(parl, par + parc);
//                }
//                if (iter == 0)
//                    par = 0;
//            }

//            return par;
//        }

//        private static void lmqrsolv(float[,] s, bool[,] perm, float[] diag, float[] qtb, float[] input, float[] sDiag)
//        {
//            /* Local variables */
//            int i, j, k, n = s.GetLength(1);
//            float temp;
//            float[] wa = new float[n];

//            /* Function Body */
//            // the following will only change the lower triangular part of s, including
//            // the diagonal, though the diagonal is restored afterward

//            //copy r and (q transpose)*b to preserve input and initialize s. */
//            //in particular, save the diagonal elements of r in x. */
//            input = s.GetDiagonal();
//            wa = qtb;


//            s.TopLeftCorner(n, n).GetStrictlyLowerView() = s.TopLeftCorner(n, n).Transpose();
//            //eliminate the diagonal matrix d using a givens rotation. */
//            for (j = 0; j < n; ++j)
//            {
//                //prepare the row of d to be eliminated, locating the */
//                //diagonal element using p from the qr factorization. */
//                int l = perm.GetIndices()[j];
//                if (diag[l] == 0)
//                {
//                    break;
//                }
//                sDiag = sDiag.SetTail(n - j, 0);
//                sDiag[j] = diag[l];

//                //the transformations to eliminate the row of d */
//                //modify only a single element of (q transpose)*b */
//                //beyond the first n, which is initially zero. */
//                float qtbpj = 0;
//                for (k = j; k < n; ++k)
//                {
//                    //determine a givens rotation which eliminates the */
//                    //appropriate element in the current row of d. */
//                    var givens = MakeGivens(-s[k, k], sDiag[k]);

//                    //compute the modified diagonal element of r and */
//                    //the modified element of ((q transpose)*b,0). */
//                    s[k, k] = givens.c * s[k, k] + givens.s * sDiag[k];
//                    temp = givens.c * wa[k] + givens.s * qtbpj;
//                    qtbpj = -givens.s * wa[k] + givens.c * qtbpj;
//                    wa[k] = temp;

//                    //accumulate the transformation in the row of s. */
//                    for (i = k + 1; i < n; ++i)
//                    {
//                        temp = givens.c * s[i, k] + givens.s * sDiag[i];
//                        sDiag[i] = -givens.s * s[i, k] + givens.c * sDiag[i];
//                        s[i, k] = temp;
//                    }
//                }
//            }

//            //solve the triangular system for z. if the system is singular, then obtain a least squares solution. */
//            int nsing;
//            for (nsing = 0; nsing < n && sDiag[nsing] != 0; nsing++) { }

//            wa = wa.SetTail(n - nsing, 0);
//            s.TopLeftCorner(nsing, nsing).Transpose().UpperTrianglarView().solveInPlace(wa.GetHead(nsing));

//            // restore
//            sDiag = s.GetDiagonal();
//            s.SetDiagonal(input);

//            /* permute the components of z back to components of x. */
//            input = perm.Multiply(wa);

//        }

//        private static (float c, float s) MakeGivens(float p, float q)
//        {
//            float c = 0, s = 0;
//            if (q == 0)
//            {
//                c = (p < 0) ? -1 : 1;
//                s = 0;
//            }
//            else if( p == 0)
//            {
//                c = 0;
//                s = -1 / MathF.Abs(q);
//            }
//            else
//            {
//                float p1 = MathF.Abs(p);
//                float q1 = MathF.Abs(q);
//                if (p1 >= q1)
//                {
//                    float ps = p / p1;
//                    float p2 = MathF.Abs(MathF.Pow(ps, 2));
//                    float qs = q / p1;
//                    float q2 = MathF.Abs(MathF.Pow(qs, 2));

//                    float u = MathF.Sqrt(1 + q2 / p2);
//                    if(p < 0)
//                    {
//                        u = -u;
//                    }
//                    c = 1 / u;
//                    s = -qs * ps * (c / p2);
//                }
//                else
//                {
//                    float ps = p / q1;
//                    float p2 = MathF.Abs(MathF.Pow(ps, 2));
//                    float qs = q / q1;
//                    float q2 = MathF.Abs(MathF.Pow(qs, 2));

//                    float u = q1 * MathF.Sqrt(p2 + q2);
//                    if (p < 0)
//                    {
//                        u = -u;
//                    }
//                    p1 = MathF.Abs(p);
//                    ps = p / p1;
//                    c = p1 / u;
//                    s = -ps * (q / u);

//                }
//            }
//            return (c, s);

//        }
//    }
//}

using System;
using SymmetryDetection.Optimisation;
using Xunit;

namespace SymmetryDetection.Test
{
    public class QRDecompositionHouseholderTests
    {
        private AlglibQRDecomposer Service { get; set; }

        public QRDecompositionHouseholderTests()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.Service = new AlglibQRDecomposer();
        }

        [Fact]
        public void Decompose_Full_Example_1()
        {
            double[,] A = new double[4, 3];
            A[0, 0] = -1;
            A[0, 1] = -1;
            A[0, 2] = 1;
            A[1, 0] = 1;
            A[1, 1] = 3;
            A[1, 2] = 3;
            A[2, 0] = -1;
            A[2, 1] = -1;
            A[2, 2] = 5;
            A[3, 0] = 1;
            A[3, 1] = 3;
            A[3, 2] = 7;

            // var errorEncountered = this.Service.Decompose(A);
            //var r = this.Service.GetR();
            double[] tau = new double[1];
            double[,] r = new double[1, 1];
            alglib.ortfac.rmatrixqr(ref A, A.GetLength(0), A.GetLength(1), ref tau, new alglib.xparams(0));
            alglib.ortfac.rmatrixqrunpackr(A, A.GetLength(0), A.GetLength(1), ref r, new alglib.xparams(0));

            //Assert.False(errorEncountered);
            Assert.Equal(4, r.GetLength(0));
            Assert.Equal(3, r.GetLength(1));
            Assert.Equal(2, Math.Round(r[0, 0]));
            Assert.Equal(4, Math.Round(r[0, 1]));// 1, 0
            Assert.Equal(2, Math.Round(r[0, 2])); //2,0
            Assert.Equal(0, Math.Round(r[1, 0]));
            Assert.Equal(-2, Math.Round(r[1, 1]));
            Assert.Equal(-8, Math.Round(r[1, 2]));
            Assert.Equal(0, Math.Round(r[2, 0]));
            Assert.Equal(0, Math.Round(r[2, 1]));
            Assert.Equal(-4, Math.Round(r[2, 2]));
            Assert.Equal(0, Math.Round(r[3, 0]));
            Assert.Equal(0, Math.Round(r[3, 1]));
            Assert.Equal(0, Math.Round(r[3, 2]));
        }

        [Fact]
        public void Decompose_Full_Example_2()
        {
            double[,] A = new double[4, 3];
            A[0, 0] = 1;
            A[0, 1] = -1;
            A[0, 2] = 4;
            A[1, 0] = 1;
            A[1, 1] = 4;
            A[1, 2] = -2;
            A[2, 0] = 1;
            A[2, 1] = 4;
            A[2, 2] = 2;
            A[3, 0] = 1;
            A[3, 1] = -1;
            A[3, 2] = 0;

            double[] tau = new double[1];
            double[,] r = new double[0, 0];
            alglib.ortfac.rmatrixqr(ref A, A.GetLength(0), A.GetLength(1), ref tau, new alglib.xparams(0));
            alglib.ortfac.rmatrixqrunpackr(A, A.GetLength(0), A.GetLength(1), ref r, new alglib.xparams(0));

            Assert.Equal(4, r.GetLength(0));
            Assert.Equal(3, r.GetLength(1));
            Assert.Equal(-2, Math.Round(r[0, 0]));
            Assert.Equal(-3, Math.Round(r[0, 1]));
            Assert.Equal(-2, Math.Round(r[0, 2]));
            Assert.Equal(0, Math.Round(r[1, 0]));
            Assert.Equal(-5, Math.Round(r[1, 1]));
            Assert.Equal(2, Math.Round(r[1, 2]));
            Assert.Equal(0, Math.Round(r[2, 0]));
            Assert.Equal(0, Math.Round(r[2, 1]));
            Assert.Equal(-4, Math.Round(r[2, 2]));
            Assert.Equal(0, Math.Round(r[3, 0]));
            Assert.Equal(0, Math.Round(r[3, 1]));
            Assert.Equal(0, Math.Round(r[3, 2]));

            Assert.Equal(4, A.GetLength(0));
            Assert.Equal(3, A.GetLength(1));
            Assert.Equal(-2, Math.Round(A[0, 0]));
            Assert.Equal(-3, Math.Round(A[0, 1]));
            Assert.Equal(-2, Math.Round(A[0, 2]));
            Assert.Equal(0, Math.Round(A[1, 0]));
            Assert.Equal(-5, Math.Round(A[1, 1]));
            Assert.Equal(2, Math.Round(A[1, 2]));
            Assert.Equal(0, Math.Round(A[2, 0]));
            Assert.Equal(0, Math.Round(A[2, 1]));
            Assert.Equal(-4, Math.Round(A[2, 2]));
            Assert.Equal(0, Math.Round(A[3, 0]));
            Assert.Equal(0, Math.Round(A[3, 1]));
            Assert.Equal(0, Math.Round(A[3, 2]));
        }

        [Fact]
        public void Decompose_Full_Example_R_And_Q()
        {
            double[,] A = new double[3, 3];
            A[0, 0] = 2;
            A[0, 1] = 1;
            A[0, 2] = 1;
            A[1, 0] = 1;
            A[1, 1] = 3;
            A[1, 2] = 2;
            A[2, 0] = 1;
            A[2, 1] = 0;
            A[2, 2] = 0;


            double[] tau = new double[1];
            double[,] r = new double[1, 1];
            double[,] q = new double[1, 1];
            alglib.ortfac.rmatrixqr(ref A, A.GetLength(0), A.GetLength(1), ref tau, new alglib.xparams(0));
            alglib.ortfac.rmatrixqrunpackr(A, A.GetLength(0), A.GetLength(1), ref r, new alglib.xparams(0));
            alglib.ortfac.rmatrixqrunpackq(A, A.GetLength(0), A.GetLength(1), tau, A.GetLength(0), ref q, new alglib.xparams(0));

            Assert.Equal(3, r.GetLength(0));
            Assert.Equal(3, r.GetLength(1));
            Assert.Equal(-2.45, Math.Round(r[0, 0], 2));
            Assert.Equal(-2.04, Math.Round(r[0, 1], 2));
            Assert.Equal(-1.63, Math.Round(r[0, 2], 2));
            Assert.Equal(0, Math.Round(r[1, 0], 2));
            Assert.Equal(-2.42, Math.Round(r[1, 1], 2));
            Assert.Equal(-1.52, Math.Round(r[1, 2], 2));
            Assert.Equal(0, Math.Round(r[2, 0], 2));
            Assert.Equal(0, Math.Round(r[2, 1], 2));
            Assert.Equal(-0.17, Math.Round(r[2, 2], 2)); // check this with R

            Assert.Equal(3, q.GetLength(0));
            Assert.Equal(3, q.GetLength(1));
            Assert.Equal(-0.82, Math.Round(q[0, 0], 2));
            Assert.Equal(0.28, Math.Round(q[0, 1], 2));
            Assert.Equal(-0.51, Math.Round(q[0, 2], 2));
            Assert.Equal(-0.41, Math.Round(q[1, 0], 2));
            Assert.Equal(-0.90, Math.Round(q[1, 1], 2));
            Assert.Equal(0.17, Math.Round(q[1, 2], 2));
            Assert.Equal(-0.41, Math.Round(q[2, 0], 2));
            Assert.Equal(0.35, Math.Round(q[2, 1], 2));
            Assert.Equal(0.85, Math.Round(q[2, 2], 2)); // check this with R as the sign is incorrect - but not on mine or the other implementation
        }

        [Fact]
        public void Decompose_Full_Example_4()
        {
            double[,] A = new double[3, 3];
            A[0, 0] = 1;
            A[0, 1] = 2;
            A[0, 2] = 0;
            A[1, 0] = -1;
            A[1, 1] = 4;
            A[1, 2] = 1;
            A[2, 0] = -3;
            A[2, 1] = 1;
            A[2, 2] = 2;

            var errorEncountered = this.Service.Decompose(ref A);
            var r = this.Service.GetR();
            var q = this.Service.GetQ();

            Assert.False(errorEncountered);
            Assert.Equal(3, r.GetLength(0));
            Assert.Equal(3, r.GetLength(1));
            Assert.Equal(-3.32, Math.Round(r[0, 0], 2));
            Assert.Equal(0, Math.Round(r[1, 0], 2));
            Assert.Equal(0, Math.Round(r[1, 0], 2));
        }
    }
}

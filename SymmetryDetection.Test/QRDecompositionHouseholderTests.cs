using System;
using SymmetryDetection.Optimisation;
using Xunit;

namespace SymmetryDetection.Test
{
    public class QRDecompositionHouseholderTests
    {
        private QRDecomposerHouseHolder Service { get; set; }

        public QRDecompositionHouseholderTests()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.Service = new QRDecomposerHouseHolder();
        }

        [Fact]
        public void Decompose_Full_Example()
        {
            float[,] A = new float[4, 3];
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

            var result = this.Service.Decompose(A);

            var qr = this.Service.GetQR();

            Assert.True(result);
            Assert.Equal(2, qr[0][0]);
            Assert.Equal(4, qr[0][1]);
            Assert.Equal(2, qr[0][2]);
            Assert.Equal(0, qr[1][0]);
            Assert.Equal(-2, qr[1][1]);
            Assert.Equal(-8, qr[1][2]);
            Assert.Equal(0, qr[2][0]);
            Assert.Equal(0, qr[2][1]);
            Assert.Equal(-4, qr[2][2]);
            Assert.Equal(0, qr[3][0]);
            Assert.Equal(0, qr[3][1]);
            Assert.Equal(0, qr[3][2]);

        }
    }
}

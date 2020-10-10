using Moq;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SymmetryDetection.Test
{
    public class ReflectionalSymmetryDetectorTests 
    {
        public ReflectionalSymmetryDetector Service { get; set; }
        private Mock<ISymmetryScoreService> ScoreServiceMock { get; set; }

        public ReflectionalSymmetryDetectorTests()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.ScoreServiceMock = new Mock<ISymmetryScoreService>();
            this.Service = new ReflectionalSymmetryDetector(this.ScoreServiceMock.Object);
        }

        [Fact]
        public void GenerateSpherePoints()
        {
            Service.GenerateSpherePoints(5);
        }
    }
}

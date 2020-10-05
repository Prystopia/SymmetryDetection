using System;
using System.Collections.Generic;
using System.Numerics;
using Moq;
using SymmetryDetection.Interfaces;
using SymmetryDetection.SymmetryDectection;
using Xunit;

namespace SymmetryDetection.Test
{
    public class DetectReflectionalSymmetryTests
    {
        public SymmetryDetectionHandler Service { get; set; }
        private Mock<IFileType> FileTypeMock { get; set; }

        private Mock<ISymmetryDetector> DetectorMock { get; set; }
        public DetectReflectionalSymmetryTests()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.FileTypeMock = new Mock<IFileType>();
            this.Service = new SymmetryDetectionHandler(this.FileTypeMock.Object, new List<ISymmetryDetector> { DetectorMock.Object });
        }

        [Fact]
        public void GetExportFile_Returns_Non_Empty_String()
        {
            this.Service.Symmetries = new List<ISymmetry>()
            {
                new ReflectionalSymmetry(new Vector3(1,1,1), new Vector3(1,1,1))
            };
            var result = this.Service.GetExportFile();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        //[Fact]
        //DetectReflectionalSymmetries
    }
}

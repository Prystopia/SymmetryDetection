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
        private Mock<ISymmetryExporter> ExporterMock { get; set; }
        public DetectReflectionalSymmetryTests()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.FileTypeMock = new Mock<IFileType>();
            this.ExporterMock = new Mock<ISymmetryExporter>();
            this.Service = new SymmetryDetectionHandler(this.FileTypeMock.Object, new List<ISymmetryDetector> { DetectorMock.Object });
        }
        //[Fact]
        //DetectReflectionalSymmetries
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using Moq;
using SymmetryDetection.DataTypes;
using SymmetryDetection.Refinement;
using SymmetryDetection.SymmetryDectection;
using Xunit;

namespace SymmetryDetection.Test
{
    public class CorrespondenceRejectorTests
    {
        public CorrespondenceRejector Service { get; set; }

        public CorrespondenceRejectorTests()
        {
            this.Setup();
        }

        private void Setup()
        {
            Service = new CorrespondenceRejector();
        }

        [Fact]
        public void Rejector_Returns_Closest_Match_For_Correspondence()
        {
            var mainGuid = Guid.Parse("54a59b08-2542-480d-9b70-b96d418eb3c1");
            var mainGuid2 = Guid.Parse("a2e0abf2-38b2-4490-aca7-9ef31d25c479");
            var correspondences = new List<Correspondence>()
            {
                new Correspondence(
                    new PointXYZRGBNormal()
                    {
                         Position = new Vector3(1, 1, 1),
                          Id = Guid.NewGuid()
                    },
                    new PointXYZRGBNormal()
                    {
                        Position = new Vector3(1,2,1),
                        Id = mainGuid
                    },
                    1),
                 new Correspondence(
                    new PointXYZRGBNormal()
                    {
                         Position = new Vector3(1, 1, 1),
                          Id = Guid.NewGuid()
                    },
                    new PointXYZRGBNormal()
                    {
                        Position = new Vector3(1,5,1),
                        Id = mainGuid
                    },
                    5),
                  new Correspondence(
                    new PointXYZRGBNormal()
                    {
                         Position = new Vector3(1, 1, 1),
                          Id = Guid.NewGuid()
                    },
                    new PointXYZRGBNormal()
                    {
                        Position = new Vector3(1,2,1),
                        Id = mainGuid2
                    },
                    2)
            };

           var remaining = Service.GetRemainingCorrespondences(correspondences);

            Assert.Equal(2, remaining.Count);
            Assert.Equal(mainGuid, remaining[0].CorrespondingPoint.Id);
            Assert.Equal(mainGuid2, remaining[1].CorrespondingPoint.Id);
            Assert.Equal(1, remaining[0].Distance);
            Assert.Equal(2, remaining[1].Distance);
        }

    }
}

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Sogeti.Planting.Tests
{
    public class PlacementReasonTests
    {
        private readonly List<Object> spawned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object asset in spawned)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }

            spawned.Clear();
        }

        private SeedDefinition NewSeed(string displayName)
        {
            SeedDefinition seed = SeedDefinition.CreateForTests(
                displayName,
                TestStages.ThreeStages(),
                TestStages.DefaultFillPerSecond,
                3f,
                1.5f);

            spawned.Add(seed);
            return seed;
        }

        [Test]
        public void Describe_SaysNothingAboutALegalSpot()
        {
            PlacementResult result = PlacementResult.Valid(Vector3.zero, Vector3.up);

            Assert.AreEqual(string.Empty, PlacementReason.Describe(result));
        }

        [Test]
        public void Describe_AsksForASeedWhenNoneIsSelected()
        {
            Assert.IsNotEmpty(PlacementReason.Describe(PlacementResult.NoSeed()));
        }

        [TestCase(PlacementStatus.NoSurface)]
        [TestCase(PlacementStatus.NotPlantableSurface)]
        public void Describe_NamesTheSoilRuleForABadSurface(PlacementStatus status)
        {
            PlacementResult result = PlacementResult.Rejected(status, Vector3.zero, Vector3.up);

            Assert.AreEqual("You can only plant on soil", PlacementReason.Describe(result));
        }

        [Test]
        public void Describe_NamesTheSlopeRule()
        {
            PlacementResult result = PlacementResult.Rejected(PlacementStatus.GroundTooSteep, Vector3.zero, Vector3.up);

            Assert.AreEqual("This ground is too steep", PlacementReason.Describe(result));
        }

        [Test]
        public void Describe_NamesTheObstacleRule()
        {
            PlacementResult result = PlacementResult.Rejected(
                PlacementStatus.BlockedByObstacle, Vector3.zero, Vector3.up);

            Assert.AreEqual("No room here", PlacementReason.Describe(result));
        }

        [Test]
        public void Describe_NamesTheNeighbourThatBlocksTheSpot()
        {
            SeedDefinition pine = NewSeed("Pine");
            PlacementResult result = PlacementResult.TooClose(Vector3.zero, Vector3.up, pine, 3f, 1f);

            // The player has to learn which neighbour costs the space, not only that a spot failed.
            StringAssert.Contains("Pine", PlacementReason.Describe(result));
        }

        [Test]
        public void Describe_FallsBackWhenTheNeighbourTypeIsUnknown()
        {
            PlacementResult result = PlacementResult.TooClose(Vector3.zero, Vector3.up, null, 3f, 1f);

            Assert.AreEqual("Another plant is too close", PlacementReason.Describe(result));
        }

        [Test]
        public void Describe_NeverReturnsNull()
        {
            foreach (PlacementStatus status in System.Enum.GetValues(typeof(PlacementStatus)))
            {
                PlacementResult result = PlacementResult.Rejected(status, Vector3.zero, Vector3.up);

                Assert.IsNotNull(PlacementReason.Describe(result), $"{status} returned null.");
            }
        }
    }
}

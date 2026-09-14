using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Sogeti.Planting.Tests
{
    public class PlantingRulesTests
    {
        private const int TerrainLayer = 3;
        private const int WaterLayer = 4;
        private const int UniversalObstacleLayer = 7;

        private readonly List<Object> spawned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            // An undestroyed ScriptableObject leaks into later editor sessions.
            foreach (Object asset in spawned)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }

            spawned.Clear();
        }

        private SeedDefinition NewSeed(
            string displayName = "Oak",
            float sameTypeSpacing = 3f,
            float otherTypeSpacing = 1.5f)
        {
            SeedDefinition seed = SeedDefinition.CreateForTests(
                displayName,
                TestStages.ThreeStages(),
                TestStages.DefaultFillPerSecond,
                sameTypeSpacing,
                otherTypeSpacing);

            spawned.Add(seed);
            return seed;
        }

        private static LayerMask MaskOf(params int[] layers)
        {
            int bits = 0;
            foreach (int layer in layers)
            {
                bits |= 1 << layer;
            }

            return bits;
        }

        #region Surface allow list

        [Test]
        public void IsPlantableSurface_AcceptsALayerInTheMask()
        {
            Assert.IsTrue(PlantingRules.IsPlantableSurface(TerrainLayer, MaskOf(TerrainLayer)));
        }

        [Test]
        public void IsPlantableSurface_RefusesWater()
        {
            Assert.IsFalse(PlantingRules.IsPlantableSurface(WaterLayer, MaskOf(TerrainLayer)));
        }

        [Test]
        public void IsPlantableSurface_RefusesAnObstacle()
        {
            Assert.IsFalse(PlantingRules.IsPlantableSurface(UniversalObstacleLayer, MaskOf(TerrainLayer)));
        }

        [Test]
        public void IsPlantableSurface_RefusesEverythingWhenTheMaskIsEmpty()
        {
            Assert.IsFalse(PlantingRules.IsPlantableSurface(TerrainLayer, MaskOf()));
        }

        [Test]
        public void IsPlantableSurface_AcceptsAnyLayerInAMultiLayerMask()
        {
            LayerMask mask = MaskOf(TerrainLayer, WaterLayer);

            Assert.IsTrue(PlantingRules.IsPlantableSurface(TerrainLayer, mask));
            Assert.IsTrue(PlantingRules.IsPlantableSurface(WaterLayer, mask));
            Assert.IsFalse(PlantingRules.IsPlantableSurface(UniversalObstacleLayer, mask));
        }

        #endregion

        #region Slope filter

        [Test]
        public void IsGroundFlatEnough_AcceptsFlatGround()
        {
            Assert.IsTrue(PlantingRules.IsGroundFlatEnough(Vector3.up, 30f));
        }

        [Test]
        public void IsGroundFlatEnough_RefusesASteepBank()
        {
            Vector3 normal = Quaternion.Euler(45f, 0f, 0f) * Vector3.up;

            Assert.IsFalse(PlantingRules.IsGroundFlatEnough(normal, 30f));
        }

        [Test]
        public void IsGroundFlatEnough_AcceptsTheToleranceItself()
        {
            Vector3 normal = Quaternion.Euler(30f, 0f, 0f) * Vector3.up;

            Assert.IsTrue(PlantingRules.IsGroundFlatEnough(normal, 30.01f));
        }

        [Test]
        public void IsGroundFlatEnough_RefusesAZeroNormal()
        {
            Assert.IsFalse(PlantingRules.IsGroundFlatEnough(Vector3.zero, 30f));
        }

        [Test]
        public void IsGroundFlatEnough_TreatsANegativeToleranceAsFlatOnly()
        {
            Assert.IsTrue(PlantingRules.IsGroundFlatEnough(Vector3.up, -10f));
            Assert.IsFalse(PlantingRules.IsGroundFlatEnough(Quaternion.Euler(5f, 0f, 0f) * Vector3.up, -10f));
        }

        #endregion

        #region Search radius

        [Test]
        public void NeighbourSearchRadius_UsesTheLargestSpacingOfTheSeed()
        {
            SeedDefinition oak = NewSeed(sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);

            float radius = PlantingRules.NeighbourSearchRadius(oak, null, 0.4f);

            Assert.That(radius, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void NeighbourSearchRadius_FallsBackToTheObstacleClearance()
        {
            SeedDefinition tiny = NewSeed(sameTypeSpacing: 0.1f, otherTypeSpacing: 0.1f);

            float radius = PlantingRules.NeighbourSearchRadius(tiny, null, 0.4f);

            Assert.That(radius, Is.EqualTo(0.4f).Within(0.0001f));
        }

        [Test]
        public void NeighbourSearchRadius_CoversAGreedierNeighbourType()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            SeedDefinition pine = NewSeed("Pine", sameTypeSpacing: 2f, otherTypeSpacing: 5f);

            float radius = PlantingRules.NeighbourSearchRadius(oak, new[] { oak, pine }, 0.4f);

            // Spacing is mutual. A sphere of 3 m would never find the pine that demands 5 m.
            Assert.That(radius, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void NeighbourSearchRadius_IgnoresEmptyCatalogSlots()
        {
            SeedDefinition oak = NewSeed(sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);

            float radius = PlantingRules.NeighbourSearchRadius(oak, new SeedDefinition[] { null, null }, 0f);

            Assert.That(radius, Is.EqualTo(3f).Within(0.0001f));
        }

        [Test]
        public void NeighbourSearchRadius_ReturnsTheClearanceWithoutASeed()
        {
            float radius = PlantingRules.NeighbourSearchRadius(null, null, 0.4f);

            Assert.That(radius, Is.EqualTo(0.4f).Within(0.0001f));
        }

        [Test]
        public void NeighbourSearchRadius_IsNeverNegative()
        {
            Assert.That(PlantingRules.NeighbourSearchRadius(null, null, -5f), Is.EqualTo(0f).Within(0.0001f));
        }

        #endregion

        #region Spacing

        [Test]
        public void CheckSpacing_RefusesWithoutASeed()
        {
            PlacementResult result = PlantingRules.CheckSpacing(null, Vector3.zero, Vector3.up, null);

            Assert.AreEqual(PlacementStatus.NoSeedSelected, result.Status);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void CheckSpacing_AcceptsAnEmptyField()
        {
            SeedDefinition oak = NewSeed();

            PlacementResult result = PlantingRules.CheckSpacing(
                oak,
                new Vector3(1f, 2f, 3f),
                Vector3.up,
                new List<NeighbourPlant>());

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), result.Position);
        }

        [Test]
        public void CheckSpacing_AcceptsANullNeighbourList()
        {
            SeedDefinition oak = NewSeed();

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, null);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void CheckSpacing_RefusesTheSameTypeInsideItsSpacing()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            var neighbours = new[] { new NeighbourPlant(oak, new Vector3(2f, 0f, 0f)) };

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, neighbours);

            Assert.AreEqual(PlacementStatus.TooCloseToPlant, result.Status);
            Assert.AreSame(oak, result.BlockingSeed);
            Assert.That(result.RequiredSpacing, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(result.ActualDistance, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void CheckSpacing_AcceptsTheSameTypeOutsideItsSpacing()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            var neighbours = new[] { new NeighbourPlant(oak, new Vector3(4f, 0f, 0f)) };

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, neighbours);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void CheckSpacing_AcceptsTheExactRequiredDistance()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            var neighbours = new[] { new NeighbourPlant(oak, new Vector3(3f, 0f, 0f)) };

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, neighbours);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void CheckSpacing_UsesTheLargerDemandBetweenTwoTypes()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            SeedDefinition pine = NewSeed("Pine", sameTypeSpacing: 2f, otherTypeSpacing: 5f);
            var neighbours = new[] { new NeighbourPlant(pine, new Vector3(4f, 0f, 0f)) };

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, neighbours);

            Assert.AreEqual(PlacementStatus.TooCloseToPlant, result.Status);
            Assert.That(result.RequiredSpacing, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void CheckSpacing_DoesNotDependOnPlantingOrder()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            SeedDefinition pine = NewSeed("Pine", sameTypeSpacing: 2f, otherTypeSpacing: 5f);
            Vector3 spot = new Vector3(4f, 0f, 0f);

            PlacementResult oakNearPine = PlantingRules.CheckSpacing(
                oak, Vector3.zero, Vector3.up, new[] { new NeighbourPlant(pine, spot) });
            PlacementResult pineNearOak = PlantingRules.CheckSpacing(
                pine, Vector3.zero, Vector3.up, new[] { new NeighbourPlant(oak, spot) });

            Assert.AreEqual(oakNearPine.Status, pineNearOak.Status);
            Assert.That(oakNearPine.RequiredSpacing, Is.EqualTo(pineNearOak.RequiredSpacing).Within(0.0001f));
        }

        [Test]
        public void CheckSpacing_IgnoresHeight()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            var neighbours = new[] { new NeighbourPlant(oak, new Vector3(0f, 10f, 0f)) };

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, neighbours);

            // Spacing is a ground footprint rule. A plant on a bank above is still the same spot.
            Assert.AreEqual(PlacementStatus.TooCloseToPlant, result.Status);
            Assert.That(result.ActualDistance, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void CheckSpacing_ReportsTheWorstOffenderNotTheFirst()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            SeedDefinition pine = NewSeed("Pine", sameTypeSpacing: 2f, otherTypeSpacing: 5f);
            var neighbours = new[]
            {
                new NeighbourPlant(oak, new Vector3(2.9f, 0f, 0f)),
                new NeighbourPlant(pine, new Vector3(0f, 0f, 1f))
            };

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, neighbours);

            Assert.AreSame(pine, result.BlockingSeed);
        }

        [Test]
        public void CheckSpacing_KeepsTheSurfaceOnARefusal()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            Vector3 point = new Vector3(5f, 1f, 5f);
            Vector3 normal = Vector3.up;
            var neighbours = new[] { new NeighbourPlant(oak, point) };

            PlacementResult result = PlantingRules.CheckSpacing(oak, point, normal, neighbours);

            Assert.AreEqual(point, result.Position);
            Assert.AreEqual(normal, result.Normal);
            Assert.IsTrue(result.HasSurface);
        }

        [Test]
        public void CheckSpacing_TreatsAMissingNeighbourSeedAsAnotherType()
        {
            SeedDefinition oak = NewSeed("Oak", sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);
            var neighbours = new[] { new NeighbourPlant(null, new Vector3(1f, 0f, 0f)) };

            PlacementResult result = PlantingRules.CheckSpacing(oak, Vector3.zero, Vector3.up, neighbours);

            Assert.AreEqual(PlacementStatus.TooCloseToPlant, result.Status);
            Assert.That(result.RequiredSpacing, Is.EqualTo(1.5f).Within(0.0001f));
        }

        #endregion

        #region Ground distance

        [Test]
        public void GroundDistance_IgnoresTheYAxis()
        {
            float distance = PlantingRules.GroundDistance(
                new Vector3(0f, 100f, 0f),
                new Vector3(3f, -100f, 4f));

            Assert.That(distance, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void GroundDistance_IsZeroForTheSameSpot()
        {
            Assert.That(PlantingRules.GroundDistance(Vector3.one, Vector3.one), Is.EqualTo(0f).Within(0.0001f));
        }

        #endregion
    }
}

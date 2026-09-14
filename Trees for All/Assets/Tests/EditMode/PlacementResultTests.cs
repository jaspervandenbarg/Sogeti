using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Sogeti.Planting.Tests
{
    public class PlacementResultTests
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

        private SeedDefinition NewSeed(string displayName = "Oak")
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

        #region Default safety

        [Test]
        public void Default_IsNotAValidSpot()
        {
            PlacementResult result = default;

            // A field that was never evaluated must never let the player plant.
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(PlacementStatus.NoSurface, result.Status);
        }

        [Test]
        public void Default_HasNoSurface()
        {
            Assert.IsFalse(default(PlacementResult).HasSurface);
        }

        #endregion

        #region Factories

        [Test]
        public void Valid_KeepsThePositionAndNormal()
        {
            Vector3 point = new Vector3(2f, 0.5f, -4f);
            Vector3 normal = Vector3.up;

            PlacementResult result = PlacementResult.Valid(point, normal);

            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.HasSurface);
            Assert.AreEqual(point, result.Position);
            Assert.AreEqual(normal, result.Normal);
            Assert.IsNull(result.BlockingSeed);
        }

        [Test]
        public void Rejected_KeepsTheSurfaceSoThePreviewStillHasASpot()
        {
            Vector3 point = new Vector3(1f, 0f, 1f);

            PlacementResult result = PlacementResult.Rejected(PlacementStatus.BlockedByObstacle, point, Vector3.up);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.HasSurface);
            Assert.AreEqual(point, result.Position);
        }

        [Test]
        public void NoSurface_HidesThePreview()
        {
            PlacementResult result = PlacementResult.NoSurface();

            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(result.HasSurface);
        }

        [Test]
        public void NoSeed_HidesThePreview()
        {
            PlacementResult result = PlacementResult.NoSeed();

            Assert.AreEqual(PlacementStatus.NoSeedSelected, result.Status);
            Assert.IsFalse(result.HasSurface);
        }

        [Test]
        public void TooClose_CarriesTheDistancesBehindTheRefusal()
        {
            SeedDefinition oak = NewSeed();

            PlacementResult result = PlacementResult.TooClose(Vector3.zero, Vector3.up, oak, 3f, 1.25f);

            Assert.AreEqual(PlacementStatus.TooCloseToPlant, result.Status);
            Assert.AreSame(oak, result.BlockingSeed);
            Assert.That(result.RequiredSpacing, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(result.ActualDistance, Is.EqualTo(1.25f).Within(0.0001f));
        }

        #endregion
    }
}

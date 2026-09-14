using NUnit.Framework;
using Sogeti.Watering;

namespace Sogeti.Planting.Tests
{
    public class WaterTankTests
    {
        private const float Capacity = 10f;

        private static WaterTank FullTank() => new WaterTank(Capacity);

        #region Draining

        [Test]
        public void Drain_FullPourForOneSecond_SpendsOneSecondOfCapacity()
        {
            WaterTank tank = FullTank();

            tank.Drain(1f, 1f);

            Assert.AreEqual(0.9f, tank.Fill01, 0.0001f);
        }

        [Test]
        public void Drain_HalfFlow_SpendsHalfAsMuch()
        {
            WaterTank tank = FullTank();

            tank.Drain(0.5f, 1f);

            Assert.AreEqual(0.95f, tank.Fill01, 0.0001f);
        }

        [Test]
        public void Drain_FullPourForTheWholeCapacity_EmptiesTheTank()
        {
            WaterTank tank = FullTank();

            tank.Drain(1f, Capacity);

            Assert.IsTrue(tank.IsEmpty);
            Assert.AreEqual(0f, tank.Fill01);
        }

        [Test]
        public void Drain_EmptyTank_ServesNoFlow()
        {
            WaterTank tank = new WaterTank(Capacity, 0f);

            float served = tank.Drain(1f, 1f);

            Assert.AreEqual(0f, served);
        }

        [Test]
        public void Drain_LastSplash_ServesLessThanTheTiltAsks()
        {
            // The stream must thin out instead of cutting off a frame late.
            WaterTank tank = new WaterTank(Capacity, 0.05f);

            float served = tank.Drain(1f, 1f);

            Assert.AreEqual(0.5f, served, 0.0001f);
            Assert.IsTrue(tank.IsEmpty);
        }

        [Test]
        public void Drain_NoTilt_ServesNoFlowAndSpendsNothing()
        {
            WaterTank tank = FullTank();

            float served = tank.Drain(0f, 1f);

            Assert.AreEqual(0f, served);
            Assert.AreEqual(1f, tank.Fill01);
        }

        #endregion

        #region Refilling

        [Test]
        public void Refill_HalfEmptyTank_AddsTheRate()
        {
            WaterTank tank = new WaterTank(Capacity, 0.5f);

            tank.Refill(0.25f, 1f);

            Assert.AreEqual(0.75f, tank.Fill01, 0.0001f);
        }

        [Test]
        public void Refill_PastTheCap_StopsAtFull()
        {
            WaterTank tank = new WaterTank(Capacity, 0.9f);

            tank.Refill(1f, 5f);

            Assert.AreEqual(1f, tank.Fill01);
            Assert.IsTrue(tank.IsFull);
        }

        #endregion

        #region Bad input

        [Test]
        public void Drain_ZeroDeltaTime_SpendsNothing()
        {
            WaterTank tank = FullTank();

            float served = tank.Drain(1f, 0f);

            Assert.AreEqual(0f, served);
            Assert.AreEqual(1f, tank.Fill01);
        }

        [Test]
        public void Drain_NegativeDeltaTime_SpendsNothing()
        {
            WaterTank tank = FullTank();

            tank.Drain(1f, -1f);

            Assert.AreEqual(1f, tank.Fill01);
        }

        [Test]
        public void Drain_NaNFlow_SpendsNothing()
        {
            WaterTank tank = FullTank();

            float served = tank.Drain(float.NaN, 1f);

            Assert.AreEqual(0f, served);
            Assert.AreEqual(1f, tank.Fill01);
        }

        [Test]
        public void Refill_NaNDeltaTime_AddsNothing()
        {
            WaterTank tank = new WaterTank(Capacity, 0.5f);

            tank.Refill(1f, float.NaN);

            Assert.AreEqual(0.5f, tank.Fill01);
        }

        [Test]
        public void Constructor_ZeroCapacity_StillDrainsWithoutDividingByZero()
        {
            WaterTank tank = new WaterTank(0f);

            tank.Drain(1f, 1f);

            Assert.IsTrue(tank.IsEmpty);
        }

        [Test]
        public void Constructor_FillAboveOne_ClampsToFull()
        {
            WaterTank tank = new WaterTank(Capacity, 5f);

            Assert.AreEqual(1f, tank.Fill01);
        }

        #endregion
    }
}

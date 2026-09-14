using NUnit.Framework;

namespace Sogeti.Planting.Tests
{
    public class WaterUrgencyBandsTests
    {
        private const float Low = 0.4f;
        private const float Critical = 0.2f;

        private static WaterUrgency Band(float water01)
        {
            return WaterUrgencyBands.For(water01, Low, Critical);
        }

        #region Bands

        [Test]
        public void For_FullMeter_IsHealthy()
        {
            Assert.AreEqual(WaterUrgency.Healthy, Band(1f));
        }

        [Test]
        public void For_FreshStage_IsHealthy()
        {
            // Every stage except the last opens at half a meter. A new plant must not read as a warning.
            Assert.AreEqual(WaterUrgency.Healthy, Band(PlantGrowth.StageStartWater));
        }

        [Test]
        public void For_JustAboveLowThreshold_IsHealthy()
        {
            Assert.AreEqual(WaterUrgency.Healthy, Band(Low + 0.01f));
        }

        [Test]
        public void For_ExactlyLowThreshold_IsLow()
        {
            Assert.AreEqual(WaterUrgency.Low, Band(Low));
        }

        [Test]
        public void For_BetweenThresholds_IsLow()
        {
            Assert.AreEqual(WaterUrgency.Low, Band(0.3f));
        }

        [Test]
        public void For_JustAboveCriticalThreshold_IsLow()
        {
            Assert.AreEqual(WaterUrgency.Low, Band(Critical + 0.01f));
        }

        [Test]
        public void For_ExactlyCriticalThreshold_IsCritical()
        {
            Assert.AreEqual(WaterUrgency.Critical, Band(Critical));
        }

        [Test]
        public void For_BelowCriticalThreshold_IsCritical()
        {
            Assert.AreEqual(WaterUrgency.Critical, Band(0.05f));
        }

        [Test]
        public void For_EmptyMeter_IsCritical()
        {
            Assert.AreEqual(WaterUrgency.Critical, Band(0f));
        }

        #endregion

        #region Bad input

        [Test]
        public void For_NaN_IsCritical()
        {
            // A broken number must read as urgent. A silent "healthy" would hide the fault.
            Assert.AreEqual(WaterUrgency.Critical, Band(float.NaN));
        }

        [Test]
        public void For_NegativeWater_ClampsToCritical()
        {
            Assert.AreEqual(WaterUrgency.Critical, Band(-5f));
        }

        [Test]
        public void For_WaterAboveOne_ClampsToHealthy()
        {
            Assert.AreEqual(WaterUrgency.Healthy, Band(5f));
        }

        [Test]
        public void For_NegativeThresholds_ClampToZero()
        {
            // Only an empty meter can then be critical, and nothing reads as low.
            Assert.AreEqual(WaterUrgency.Critical, WaterUrgencyBands.For(0f, -1f, -1f));
            Assert.AreEqual(WaterUrgency.Healthy, WaterUrgencyBands.For(0.01f, -1f, -1f));
        }

        [Test]
        public void For_LowThresholdUnderCriticalThreshold_LeavesNoLowBand()
        {
            // The tighter value wins, so a mis-tuned pair never swallows the critical band.
            Assert.AreEqual(WaterUrgency.Critical, WaterUrgencyBands.For(0.5f, 0.2f, 0.6f));
            Assert.AreEqual(WaterUrgency.Healthy, WaterUrgencyBands.For(0.7f, 0.2f, 0.6f));
        }

        #endregion

        #region Defaults

        [Test]
        public void For_WithoutThresholds_UsesTheDefaults()
        {
            Assert.AreEqual(
                WaterUrgencyBands.For(0.3f, WaterUrgencyBands.DefaultLowThreshold, WaterUrgencyBands.DefaultCriticalThreshold),
                WaterUrgencyBands.For(0.3f));
        }

        [Test]
        public void Defaults_LeaveTheFreshStageHealthy()
        {
            Assert.Less(WaterUrgencyBands.DefaultLowThreshold, PlantGrowth.StageStartWater);
            Assert.Less(WaterUrgencyBands.DefaultCriticalThreshold, WaterUrgencyBands.DefaultLowThreshold);
        }

        #endregion
    }
}

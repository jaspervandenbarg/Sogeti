using NUnit.Framework;

namespace Sogeti.Planting.Tests
{
    public class GrowthPointsTests
    {
        private const int Base = 40;
        private const float Grace = 10f;
        private const float Decay = 30f;
        private const float Floor = 0.25f;

        private static int Award(float secondsInStage)
        {
            return GrowthPoints.Award(Base, secondsInStage, Grace, Decay, Floor);
        }

        [Test]
        public void Award_WithZeroElapsed_GivesFullPoints()
        {
            Assert.AreEqual(Base, Award(0f));
        }

        [Test]
        public void Award_InsideGrace_GivesFullPoints()
        {
            Assert.AreEqual(Base, Award(Grace * 0.5f));
        }

        [Test]
        public void Award_AtTheGraceBoundary_GivesFullPoints()
        {
            Assert.AreEqual(Base, Award(Grace));
        }

        [Test]
        public void Award_AtHalfTheDecay_GivesTheMidpoint()
        {
            // Halfway between 40 and 10.
            Assert.AreEqual(25, Award(Grace + Decay * 0.5f));
        }

        [Test]
        public void Award_AtTheEndOfDecay_GivesTheMinimumFraction()
        {
            Assert.AreEqual(10, Award(Grace + Decay));
        }

        [Test]
        public void Award_LongAfterDecay_StaysAtTheMinimum()
        {
            Assert.AreEqual(10, Award(Grace + Decay * 100f));
        }

        [Test]
        public void Award_WithZeroDecaySeconds_DropsStraightToTheMinimum()
        {
            Assert.AreEqual(10, GrowthPoints.Award(Base, Grace + 0.01f, Grace, 0f, Floor));
        }

        [Test]
        public void Award_WithNegativeElapsed_GivesFullPoints()
        {
            Assert.AreEqual(Base, Award(-5f));
        }

        [Test]
        public void Award_WithZeroBasePoints_ReturnsZero()
        {
            Assert.AreEqual(0, GrowthPoints.Award(0, 999f, Grace, Decay, Floor));
        }

        [Test]
        public void Award_WithNegativeBasePoints_ReturnsZero()
        {
            Assert.AreEqual(0, GrowthPoints.Award(-10, 0f, Grace, Decay, Floor));
        }

        [Test]
        public void Award_ClampsMinimumFractionAboveOne()
        {
            Assert.AreEqual(Base, GrowthPoints.Award(Base, 999f, Grace, Decay, 5f));
        }

        [Test]
        public void Award_ClampsMinimumFractionBelowZero()
        {
            Assert.AreEqual(0, GrowthPoints.Award(Base, 999f, Grace, Decay, -5f));
        }

        [Test]
        public void Award_NeverReturnsNegative()
        {
            Assert.GreaterOrEqual(GrowthPoints.Award(Base, 10000f, 0f, 1f, 0f), 0);
        }

        [Test]
        public void Award_WithNegativeGrace_StillDecays()
        {
            Assert.Less(GrowthPoints.Award(Base, 15f, -10f, Decay, Floor), Base);
        }

        [Test]
        public void Award_DecreasesAsElapsedTimeGrows()
        {
            float[] elapsedValues = { 0f, 10f, 20f, 30f, 40f, 60f, 120f };

            int previous = int.MaxValue;
            foreach (float elapsed in elapsedValues)
            {
                int award = Award(elapsed);
                Assert.LessOrEqual(award, previous, $"Award rose at {elapsed} s.");
                previous = award;
            }
        }

        [Test]
        public void Award_ForEqualTime_IsLargerOnTheRicherStage()
        {
            int cheap = GrowthPoints.Award(20, 20f, Grace, Decay, Floor);
            int rich = GrowthPoints.Award(40, 20f, Grace, Decay, Floor);
            Assert.Greater(rich, cheap);
        }

        [Test]
        public void Award_FromStage_ReadsTheStageValues()
        {
            GrowthStage stage = TestStages.Stage(points: 40, grace: 10f, decay: 30f, floor: 0.25f);
            Assert.AreEqual(40, GrowthPoints.Award(stage, 0f));
            Assert.AreEqual(10, GrowthPoints.Award(stage, 40f));
        }

        [Test]
        public void Award_FromNullStage_ReturnsZero()
        {
            Assert.AreEqual(0, GrowthPoints.Award(null, 0f));
        }

        [Test]
        public void Award_RoundsToTheNearestInteger()
        {
            // The ramp drops 1 point per second here, so 7.3 s past grace lands on 32.7.
            Assert.AreEqual(33, Award(Grace + 7.3f));
        }
    }
}

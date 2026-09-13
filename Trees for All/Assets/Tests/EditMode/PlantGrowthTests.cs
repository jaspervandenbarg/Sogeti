using System;
using NUnit.Framework;

namespace Sogeti.Planting.Tests
{
    public class PlantGrowthTests
    {
        private const float Tolerance = 0.001f;

        // Stage 1 drains 1/30 per second, so a full meter takes 30 s and a half meter 15 s.
        private const float Stage1SecondsToEmpty = 30f;
        private const float Stage1SecondsFromHalf = 15f;

        private const float FillFlow = 1f;

        // 0.25/s in minus 1/30 per second out nets ~0.2167/s, so half to full takes ~2.3 s.
        // Watering longer than this flips a second stage and breaks single-advance tests.
        private const float OneStageOfWatering = 3f;

        #region Start state

        [Test]
        public void NewPlant_StartsAtFirstStage()
        {
            Assert.AreEqual(0, TestStages.Growth().CurrentStageIndex);
        }

        [Test]
        public void NewPlant_StartsWithHalfWater()
        {
            Assert.AreEqual(PlantGrowth.StageStartWater, TestStages.Growth().Water01, Tolerance);
        }

        [Test]
        public void NewPlant_HasWaterMeter()
        {
            PlantGrowth growth = TestStages.Growth();
            Assert.IsTrue(growth.HasWaterMeter);
            Assert.IsFalse(growth.IsDead);
            Assert.IsFalse(growth.IsFullyGrown);
        }

        [Test]
        public void NewPlant_StartsWithNoElapsedTime()
        {
            Assert.AreEqual(0f, TestStages.Growth().SecondsInCurrentStage, Tolerance);
        }

        [Test]
        public void Constructor_WithNullStages_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PlantGrowth(null, 0.25f));
        }

        [Test]
        public void Constructor_WithEmptyStages_Throws()
        {
            Assert.Throws<ArgumentException>(() => new PlantGrowth(new GrowthStage[0], 0.25f));
        }

        [Test]
        public void Constructor_WithOneStage_IsFullyGrownAtOnce()
        {
            PlantGrowth growth = new PlantGrowth(new[] { TestStages.Stage() }, 0.25f);

            Assert.IsTrue(growth.IsFullyGrown);
            Assert.IsFalse(growth.HasWaterMeter);
            Assert.AreEqual(0f, growth.Water01, Tolerance);
        }

        [Test]
        public void Constructor_ClampsNegativeFillRateToZero()
        {
            Assert.AreEqual(0f, new PlantGrowth(TestStages.ThreeStages(), -5f).WaterFillPerSecond, Tolerance);
        }

        #endregion

        #region Drain to death

        [Test]
        public void Tick_WithoutWater_DrainsAtStageRate()
        {
            PlantGrowth growth = TestStages.Growth();
            growth.Tick(3f);

            Assert.AreEqual(0.5f - 3f / Stage1SecondsToEmpty, growth.Water01, Tolerance);
        }

        [Test]
        public void Tick_WithoutWater_ForHalfTheStageDuration_Dies()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, Stage1SecondsFromHalf + 0.1f);

            Assert.IsTrue(growth.IsDead);
        }

        [Test]
        public void Tick_WithoutWater_JustBeforeTheDeadline_Survives()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, Stage1SecondsFromHalf - 1f);

            Assert.IsFalse(growth.IsDead);
        }

        [Test]
        public void Died_FiresOnce_WithTheStageIndex()
        {
            PlantGrowth growth = TestStages.Growth();
            int calls = 0;
            int reportedStage = -1;
            growth.Died += stage =>
            {
                calls++;
                reportedStage = stage;
            };

            TestStages.TickFor(growth, 60f);

            Assert.AreEqual(1, calls);
            Assert.AreEqual(0, reportedStage);
        }

        [Test]
        public void Death_LeavesWaterAtZero()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, 60f);

            Assert.AreEqual(0f, growth.Water01, Tolerance);
        }

        [Test]
        public void SecondStage_DrainsSlowerThanFirstStage()
        {
            GrowthStage[] stages = TestStages.ThreeStages();
            Assert.Less(stages[1].DrainPerSecond, stages[0].DrainPerSecond);
        }

        #endregion

        #region Fill to advance

        [Test]
        public void Tick_WithFullFlow_RaisesWater()
        {
            PlantGrowth growth = TestStages.Growth();
            growth.Tick(1f, FillFlow);

            Assert.Greater(growth.Water01, 0.5f);
        }

        [Test]
        public void Tick_WithFullFlow_AdvancesTheStage()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, OneStageOfWatering, FillFlow);

            Assert.AreEqual(1, growth.CurrentStageIndex);
        }

        [Test]
        public void StageAdvanced_ReportsTheNewIndex()
        {
            PlantGrowth growth = TestStages.Growth();
            int reportedIndex = -1;
            growth.StageAdvanced += (index, _) => reportedIndex = index;

            TestStages.TickFor(growth, OneStageOfWatering, FillFlow);

            Assert.AreEqual(1, reportedIndex);
        }

        [Test]
        public void StageAdvanced_ReportsSecondsSpentInThePreviousStage()
        {
            PlantGrowth growth = TestStages.Growth();
            float reportedSeconds = -1f;
            growth.StageAdvanced += (_, seconds) => reportedSeconds = seconds;

            // Coast for 5 s, then water until the stage flips at about 8 s total.
            TestStages.TickFor(growth, 5f);
            TestStages.TickFor(growth, 4f, FillFlow);

            Assert.Greater(reportedSeconds, 5f);
            Assert.Less(reportedSeconds, 9f);
        }

        [Test]
        public void Tick_WithHalfFlow_FillsSlowerThanFullFlow()
        {
            PlantGrowth fast = TestStages.Growth();
            PlantGrowth slow = TestStages.Growth();

            fast.Tick(1f, 1f);
            slow.Tick(1f, 0.5f);

            Assert.Greater(fast.Water01, slow.Water01);
        }

        [Test]
        public void Tick_WithFlowEqualToDrain_HoldsTheWaterLevel()
        {
            // 1/30 per second in exactly cancels the stage 1 drain.
            PlantGrowth growth = new PlantGrowth(TestStages.ThreeStages(), 1f / Stage1SecondsToEmpty);
            TestStages.TickFor(growth, 20f, 1f);

            Assert.AreEqual(0.5f, growth.Water01, 0.01f);
        }

        [Test]
        public void Tick_WithFlowEqualToDrain_NeverAdvances()
        {
            PlantGrowth growth = new PlantGrowth(TestStages.ThreeStages(), 1f / Stage1SecondsToEmpty);
            TestStages.TickFor(growth, 120f, 1f);

            Assert.AreEqual(0, growth.CurrentStageIndex);
            Assert.IsFalse(growth.IsDead);
        }

        #endregion

        #region The 50% reset

        [Test]
        public void StageAdvance_ResetsWaterToHalf()
        {
            PlantGrowth growth = TestStages.Growth();
            growth.Tick(3f, FillFlow);

            Assert.AreEqual(1, growth.CurrentStageIndex);
            Assert.AreEqual(PlantGrowth.StageStartWater, growth.Water01, Tolerance);
        }

        [Test]
        public void StageAdvance_DiscardsSurplusWater()
        {
            PlantGrowth growth = TestStages.Growth();
            // One huge tick would overfill many times over. The surplus must not carry.
            growth.Tick(1000f, FillFlow);

            Assert.AreEqual(1, growth.CurrentStageIndex);
            Assert.AreEqual(PlantGrowth.StageStartWater, growth.Water01, Tolerance);
        }

        [Test]
        public void StageAdvance_ResetsSecondsInCurrentStage()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, OneStageOfWatering, FillFlow);

            Assert.AreEqual(1, growth.CurrentStageIndex);
            Assert.Less(growth.SecondsInCurrentStage, OneStageOfWatering);
        }

        #endregion

        #region The last stage has no meter

        [Test]
        public void ReachingTheLastStage_ClearsHasWaterMeter()
        {
            PlantGrowth growth = GrowToLastStage();

            Assert.IsFalse(growth.HasWaterMeter);
        }

        [Test]
        public void ReachingTheLastStage_SetsIsFullyGrown()
        {
            PlantGrowth growth = GrowToLastStage();

            Assert.IsTrue(growth.IsFullyGrown);
            Assert.AreEqual(growth.StageCount - 1, growth.CurrentStageIndex);
        }

        [Test]
        public void LastStage_DoesNotDrain()
        {
            PlantGrowth growth = GrowToLastStage();
            float water = growth.Water01;

            TestStages.TickFor(growth, 300f);

            Assert.AreEqual(water, growth.Water01, Tolerance);
        }

        [Test]
        public void LastStage_Tick_FiresNoEvents()
        {
            PlantGrowth growth = GrowToLastStage();
            growth.StageAdvanced += (_, __) => Assert.Fail("A fully grown plant advanced again.");
            growth.Died += _ => Assert.Fail("A fully grown plant died.");

            TestStages.TickFor(growth, 300f, FillFlow);
        }

        [Test]
        public void LastStage_CannotDie()
        {
            PlantGrowth growth = GrowToLastStage();
            TestStages.TickFor(growth, 600f);

            Assert.IsFalse(growth.IsDead);
        }

        #endregion

        #region Edge cases

        [Test]
        public void Tick_WithHugeDeltaTime_AdvancesAtMostOneStage()
        {
            PlantGrowth growth = TestStages.Growth();
            growth.Tick(100000f, FillFlow);

            Assert.AreEqual(1, growth.CurrentStageIndex);
        }

        [Test]
        public void Tick_WithHugeDeltaTime_AndNoWater_DiesInTheCurrentStage()
        {
            PlantGrowth growth = TestStages.Growth();
            int reportedStage = -1;
            growth.Died += stage => reportedStage = stage;

            growth.Tick(100000f);

            Assert.IsTrue(growth.IsDead);
            Assert.AreEqual(0, reportedStage);
            Assert.AreEqual(0, growth.CurrentStageIndex);
        }

        [Test]
        public void Tick_AfterDeath_ChangesNothing()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, 60f);

            int stage = growth.CurrentStageIndex;
            growth.Tick(1f, FillFlow);

            Assert.AreEqual(stage, growth.CurrentStageIndex);
            Assert.AreEqual(0f, growth.Water01, Tolerance);
        }

        [Test]
        public void Tick_AfterDeath_WithFullFlow_DoesNotRevive()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, 60f);
            TestStages.TickFor(growth, 60f, FillFlow);

            Assert.IsTrue(growth.IsDead);
            Assert.IsFalse(growth.HasWaterMeter);
        }

        [Test]
        public void Tick_AfterDeath_FiresNoSecondDiedEvent()
        {
            PlantGrowth growth = TestStages.Growth();
            int calls = 0;
            growth.Died += _ => calls++;

            TestStages.TickFor(growth, 60f);
            TestStages.TickFor(growth, 60f);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void DeadPlant_IsNotFullyGrown()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, 60f);

            Assert.IsTrue(growth.IsDead);
            Assert.IsFalse(growth.IsFullyGrown);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Tick_WithInvalidDeltaTime_ChangesNothing(float deltaTime)
        {
            PlantGrowth growth = TestStages.Growth();
            growth.Tick(deltaTime, FillFlow);

            Assert.AreEqual(PlantGrowth.StageStartWater, growth.Water01, Tolerance);
            Assert.AreEqual(0f, growth.SecondsInCurrentStage, Tolerance);
            Assert.AreEqual(0, growth.CurrentStageIndex);
        }

        [Test]
        public void Tick_WithFlowAboveOne_ClampsTheFlow()
        {
            PlantGrowth clamped = TestStages.Growth();
            PlantGrowth full = TestStages.Growth();

            clamped.Tick(1f, 1000f);
            full.Tick(1f, 1f);

            Assert.AreEqual(full.Water01, clamped.Water01, Tolerance);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Tick_WithInvalidFlow_TreatsItAsNoWater(float flow)
        {
            PlantGrowth invalid = TestStages.Growth();
            PlantGrowth dry = TestStages.Growth();

            invalid.Tick(1f, flow);
            dry.Tick(1f, 0f);

            Assert.AreEqual(dry.Water01, invalid.Water01, Tolerance);
        }

        [Test]
        public void Water01_StaysInRange_AcrossManyTicks()
        {
            PlantGrowth growth = TestStages.Growth();
            for (int i = 0; i < 2000; i++)
            {
                growth.Tick(0.02f, i % 3 == 0 ? 1f : 0f);
                Assert.GreaterOrEqual(growth.Water01, 0f);
                Assert.LessOrEqual(growth.Water01, 1f);
            }
        }

        [Test]
        public void ManySmallTicks_MatchOneLargeTick_ForDrain()
        {
            PlantGrowth stepped = TestStages.Growth();
            PlantGrowth single = TestStages.Growth();

            TestStages.TickFor(stepped, 6f);
            single.Tick(6f);

            Assert.AreEqual(single.Water01, stepped.Water01, 0.01f);
        }

        #endregion

        // Waters through every stage until the plant reaches the last one.
        private static PlantGrowth GrowToLastStage()
        {
            PlantGrowth growth = TestStages.Growth();
            TestStages.TickFor(growth, 60f, FillFlow);

            Assert.IsTrue(growth.IsFullyGrown, "Setup failed, the plant never reached the last stage.");
            return growth;
        }
    }
}

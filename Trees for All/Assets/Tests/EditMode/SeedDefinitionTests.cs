using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Sogeti.Planting.Tests
{
    public class SeedDefinitionTests
    {
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
            GrowthStage[] stages = null,
            float fillPerSecond = TestStages.DefaultFillPerSecond,
            float sameTypeSpacing = 3f,
            float otherTypeSpacing = 1.5f,
            GameObject defaultDeath = null)
        {
            SeedDefinition seed = SeedDefinition.CreateForTests(
                displayName,
                stages ?? TestStages.ThreeStages(),
                fillPerSecond,
                sameTypeSpacing,
                otherTypeSpacing,
                defaultDeath);

            spawned.Add(seed);
            return seed;
        }

        private GameObject NewPrefabStandIn(string name)
        {
            GameObject prefab = new GameObject(name);
            spawned.Add(prefab);
            return prefab;
        }

        #region Stage lookup

        [Test]
        public void GetStage_ReturnsTheStageAtTheIndex()
        {
            GrowthStage[] stages = TestStages.ThreeStages();
            SeedDefinition seed = NewSeed(stages: stages);

            Assert.AreSame(stages[1], seed.GetStage(1));
        }

        [TestCase(-1)]
        [TestCase(3)]
        [TestCase(99)]
        public void GetStage_OutOfRange_ReturnsNull(int index)
        {
            Assert.IsNull(NewSeed().GetStage(index));
        }

        [Test]
        public void StageCount_MatchesTheStageArray()
        {
            Assert.AreEqual(3, NewSeed().StageCount);
        }

        #endregion

        #region Death visuals

        [Test]
        public void GetDeathVisualPrefab_UsesTheStageOverride()
        {
            GameObject dryTree = NewPrefabStandIn("dryTree");
            GameObject dryBranches = NewPrefabStandIn("dryBranches");

            GrowthStage[] stages = TestStages.ThreeStages();
            stages[2].SetVisualsForTests(null, dryTree);

            SeedDefinition seed = NewSeed(stages: stages, defaultDeath: dryBranches);

            Assert.AreSame(dryTree, seed.GetDeathVisualPrefab(2));
        }

        [Test]
        public void GetDeathVisualPrefab_FallsBackToTheDefault()
        {
            GameObject dryBranches = NewPrefabStandIn("dryBranches");
            SeedDefinition seed = NewSeed(defaultDeath: dryBranches);

            // A seed that dies as grass must not pop into a full dry tree.
            Assert.AreSame(dryBranches, seed.GetDeathVisualPrefab(0));
            Assert.AreSame(dryBranches, seed.GetDeathVisualPrefab(1));
        }

        [Test]
        public void GetDeathVisualPrefab_OutOfRange_ReturnsTheDefault()
        {
            GameObject dryBranches = NewPrefabStandIn("dryBranches");
            SeedDefinition seed = NewSeed(defaultDeath: dryBranches);

            Assert.AreSame(dryBranches, seed.GetDeathVisualPrefab(99));
        }

        #endregion

        #region Spacing

        [Test]
        public void RequiredSpacingTo_Itself_UsesTheSameTypeValue()
        {
            SeedDefinition oak = NewSeed(sameTypeSpacing: 3f, otherTypeSpacing: 1.5f);

            Assert.AreEqual(3f, oak.RequiredSpacingTo(oak));
        }

        [Test]
        public void RequiredSpacingTo_AnotherType_UsesTheLargerOtherValue()
        {
            SeedDefinition oak = NewSeed("Oak", otherTypeSpacing: 1.5f);
            SeedDefinition pine = NewSeed("Pine", otherTypeSpacing: 4f);

            Assert.AreEqual(4f, oak.RequiredSpacingTo(pine));
        }

        [Test]
        public void RequiredSpacingTo_IsSymmetric()
        {
            SeedDefinition oak = NewSeed("Oak", otherTypeSpacing: 1.5f);
            SeedDefinition pine = NewSeed("Pine", otherTypeSpacing: 4f);

            Assert.AreEqual(oak.RequiredSpacingTo(pine), pine.RequiredSpacingTo(oak));
        }

        [Test]
        public void RequiredSpacingTo_Null_UsesTheOtherTypeValue()
        {
            SeedDefinition oak = NewSeed(otherTypeSpacing: 1.5f);

            Assert.AreEqual(1.5f, oak.RequiredSpacingTo(null));
        }

        [Test]
        public void LargestSpacing_ReturnsTheLargerOfBothValues()
        {
            Assert.AreEqual(3f, NewSeed(sameTypeSpacing: 3f, otherTypeSpacing: 1.5f).LargestSpacing);
            Assert.AreEqual(5f, NewSeed(sameTypeSpacing: 2f, otherTypeSpacing: 5f).LargestSpacing);
        }

        #endregion

        #region Growth creation

        [Test]
        public void CreateGrowth_UsesTheDefinitionStages()
        {
            PlantGrowth growth = NewSeed().CreateGrowth();

            Assert.AreEqual(3, growth.StageCount);
            Assert.AreEqual(0, growth.CurrentStageIndex);
            Assert.IsTrue(growth.HasWaterMeter);
        }

        [Test]
        public void CreateGrowth_UsesTheDefinitionFillRate()
        {
            Assert.AreEqual(0.4f, NewSeed(fillPerSecond: 0.4f).CreateGrowth().WaterFillPerSecond, 0.001f);
        }

        [Test]
        public void CreateGrowth_ReturnsAnIndependentInstance()
        {
            SeedDefinition seed = NewSeed();
            PlantGrowth first = seed.CreateGrowth();
            PlantGrowth second = seed.CreateGrowth();

            first.Tick(5f);

            Assert.AreNotSame(first, second);
            Assert.AreEqual(PlantGrowth.StageStartWater, second.Water01, 0.001f);
        }

        #endregion

        #region Validation

        [Test]
        public void OnValidate_ClampsNegativeSpacingToZero()
        {
            SeedDefinition seed = NewSeed(sameTypeSpacing: -3f, otherTypeSpacing: -1f);
            seed.ValidateNow();

            Assert.AreEqual(0f, seed.MinimumSpacingToSameType);
            Assert.AreEqual(0f, seed.MinimumSpacingToOtherTypes);
        }

        [Test]
        public void OnValidate_WarnsWhenAStageCanNeverFill()
        {
            // Stage 1 drains 1/30 per second, so a 0.01/s can never fills it.
            SeedDefinition seed = NewSeed(fillPerSecond: 0.01f);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("stage 1 can never fill"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("stage 2 can never fill"));

            seed.ValidateNow();
        }

        [Test]
        public void OnValidate_DoesNotWarnForTheLastStage()
        {
            SeedDefinition seed = NewSeed();
            seed.ValidateNow();

            // The last stage has no meter, so its zero drain is correct, not a fault.
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void OnValidate_WarnsWhenTheStageCountIsNotThree()
        {
            SeedDefinition seed = NewSeed(stages: new[] { TestStages.Stage(), TestStages.Stage(secondsToEmpty: 0f) });

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("3 growth stages"));

            seed.ValidateNow();
        }

        [Test]
        public void CanFill_IsFalseWhenWateringOnlyMatchesTheDrain()
        {
            GrowthStage stage = TestStages.Stage(secondsToEmpty: 30f);

            Assert.IsFalse(stage.CanFill(1f / 30f));
            Assert.IsTrue(stage.CanFill(0.25f));
        }

        #endregion
    }
}

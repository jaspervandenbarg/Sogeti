using System.Collections.Generic;
using NUnit.Framework;
using Sogeti.Game;
using UnityEngine;

namespace Sogeti.Planting.Tests
{
    public class ScoreTallyTests
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

        #region Total

        [Test]
        public void New_StartsEmpty()
        {
            ScoreTally tally = new ScoreTally();

            Assert.AreEqual(0, tally.Total);
            Assert.AreEqual(0, tally.PlantsPlanted);
            Assert.AreEqual(0, tally.TreesFullyGrown);
            Assert.AreEqual(0, tally.PlantsLost);
            Assert.AreEqual(0, tally.PerSeed.Count);
        }

        [Test]
        public void Add_Accumulates()
        {
            ScoreTally tally = new ScoreTally();

            tally.Add(10);
            tally.Add(20);
            tally.Add(40);

            Assert.AreEqual(70, tally.Total);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Add_NonPositive_IsRefused(int points)
        {
            ScoreTally tally = new ScoreTally();
            tally.Add(10);

            tally.Add(points);

            Assert.AreEqual(10, tally.Total);
        }

        [Test]
        public void Add_RaisesTotalChangedWithTheNewTotal()
        {
            ScoreTally tally = new ScoreTally();
            int seen = 0;
            tally.TotalChanged += total => seen = total;

            tally.Add(25);

            Assert.AreEqual(25, seen);
        }

        [Test]
        public void Add_RefusedPoints_RaisesNothing()
        {
            ScoreTally tally = new ScoreTally();
            int raised = 0;
            tally.TotalChanged += _ => raised++;

            tally.Add(0);
            tally.Add(-5);

            Assert.AreEqual(0, raised);
        }

        #endregion

        #region Counters

        [Test]
        public void RecordPlanted_CountsAndScores()
        {
            ScoreTally tally = new ScoreTally();
            SeedDefinition oak = NewSeed("Oak");

            tally.RecordPlanted(oak, 10);
            tally.RecordPlanted(oak, 10);

            Assert.AreEqual(2, tally.PlantsPlanted);
            Assert.AreEqual(20, tally.Total);
        }

        [Test]
        public void RecordStageReached_NotTheLastStage_ScoresWithoutCountingATree()
        {
            ScoreTally tally = new ScoreTally();
            SeedDefinition oak = NewSeed("Oak");

            tally.RecordStageReached(oak, 20, fullyGrown: false);

            Assert.AreEqual(20, tally.Total);
            Assert.AreEqual(0, tally.TreesFullyGrown);
        }

        [Test]
        public void RecordStageReached_TheLastStage_CountsATree()
        {
            ScoreTally tally = new ScoreTally();
            SeedDefinition oak = NewSeed("Oak");

            tally.RecordStageReached(oak, 40, fullyGrown: true);

            Assert.AreEqual(40, tally.Total);
            Assert.AreEqual(1, tally.TreesFullyGrown);
        }

        [Test]
        public void RecordLost_CountsWithoutTouchingTheScore()
        {
            ScoreTally tally = new ScoreTally();
            tally.Add(50);

            tally.RecordLost();
            tally.RecordLost();

            Assert.AreEqual(2, tally.PlantsLost);
            Assert.AreEqual(50, tally.Total);
        }

        #endregion

        #region Per seed

        [Test]
        public void RecordPlanted_AddsOneRowPerSeedType()
        {
            ScoreTally tally = new ScoreTally();
            SeedDefinition oak = NewSeed("Oak");
            SeedDefinition pine = NewSeed("Pine");

            tally.RecordPlanted(oak, 10);
            tally.RecordPlanted(oak, 10);
            tally.RecordPlanted(pine, 10);

            Assert.AreEqual(2, tally.PerSeed.Count);
        }

        [Test]
        public void PerSeed_KeepsTwoSeedTypesApart()
        {
            ScoreTally tally = new ScoreTally();
            SeedDefinition oak = NewSeed("Oak");
            SeedDefinition pine = NewSeed("Pine");

            tally.RecordPlanted(oak, 10);
            tally.RecordPlanted(oak, 10);
            tally.RecordPlanted(pine, 10);
            tally.RecordStageReached(oak, 40, fullyGrown: true);

            SeedTally oakRow = tally.PerSeed[0];
            SeedTally pineRow = tally.PerSeed[1];

            Assert.AreEqual(oak, oakRow.Seed);
            Assert.AreEqual(2, oakRow.Planted);
            Assert.AreEqual(1, oakRow.FullyGrown);

            Assert.AreEqual(pine, pineRow.Seed);
            Assert.AreEqual(1, pineRow.Planted);
            Assert.AreEqual(0, pineRow.FullyGrown);
        }

        [Test]
        public void PerSeed_HoldsFirstPlantOrder()
        {
            ScoreTally tally = new ScoreTally();
            SeedDefinition pine = NewSeed("Pine");
            SeedDefinition oak = NewSeed("Oak");

            tally.RecordPlanted(pine, 10);
            tally.RecordPlanted(oak, 10);
            tally.RecordPlanted(pine, 10);

            Assert.AreEqual(pine, tally.PerSeed[0].Seed);
            Assert.AreEqual(oak, tally.PerSeed[1].Seed);
        }

        [Test]
        public void PerSeed_BeforeAnyPlant_IsEmpty()
        {
            ScoreTally tally = new ScoreTally();

            tally.Add(10);

            Assert.AreEqual(0, tally.PerSeed.Count);
        }

        [Test]
        public void RecordStageReached_NullSeed_StillScores()
        {
            ScoreTally tally = new ScoreTally();

            tally.RecordStageReached(null, 40, fullyGrown: true);

            Assert.AreEqual(40, tally.Total);
            Assert.AreEqual(1, tally.TreesFullyGrown);
            Assert.AreEqual(0, tally.PerSeed.Count);
        }

        #endregion

        #region Reset

        [Test]
        public void Reset_EmptiesEveryCount()
        {
            ScoreTally tally = new ScoreTally();
            SeedDefinition oak = NewSeed("Oak");
            tally.RecordPlanted(oak, 10);
            tally.RecordStageReached(oak, 40, fullyGrown: true);
            tally.RecordLost();

            tally.Reset();

            Assert.AreEqual(0, tally.Total);
            Assert.AreEqual(0, tally.PlantsPlanted);
            Assert.AreEqual(0, tally.TreesFullyGrown);
            Assert.AreEqual(0, tally.PlantsLost);
            Assert.AreEqual(0, tally.PerSeed.Count);
        }

        [Test]
        public void Reset_RaisesTotalChangedOnce()
        {
            ScoreTally tally = new ScoreTally();
            tally.Add(30);
            int raised = 0;
            tally.TotalChanged += _ => raised++;

            tally.Reset();

            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Reset_AnAlreadyEmptyTally_RaisesNothing()
        {
            ScoreTally tally = new ScoreTally();
            int raised = 0;
            tally.TotalChanged += _ => raised++;

            tally.Reset();

            Assert.AreEqual(0, raised);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using Sogeti.Planting;

namespace Sogeti.Game
{
    /// <summary>
    /// The score for one round, plus the counts the game over panel reports.
    /// Every point value arrives from a Plant or SeedPlanter event, already
    /// computed by GrowthPoints. This type never recomputes an award and never
    /// polls a plant.
    /// </summary>
    public class ScoreTally
    {
        private readonly List<SeedTally> perSeed = new List<SeedTally>();

        /// <summary>The new total. Raised only when the total actually moves.</summary>
        public event Action<int> TotalChanged;

        public int Total { get; private set; }

        public int PlantsPlanted { get; private set; }

        public int TreesFullyGrown { get; private set; }

        public int PlantsLost { get; private set; }

        /// <summary>
        /// One entry per seed type the player planted, in first-plant order.
        /// A list, not a dictionary, so the panel draws a stable order and reading
        /// it allocates no enumerator.
        /// </summary>
        public IReadOnlyList<SeedTally> PerSeed => perSeed;

        public void RecordPlanted(SeedDefinition seed, int points)
        {
            PlantsPlanted++;
            TallyFor(seed)?.RecordPlanted();
            Add(points);
        }

        /// <summary>
        /// One growth stage reached. The caller says whether that stage was the last,
        /// because Plant.IsFullyGrown is the only source of that truth.
        /// </summary>
        public void RecordStageReached(SeedDefinition seed, int points, bool fullyGrown)
        {
            if (fullyGrown)
            {
                TreesFullyGrown++;
                TallyFor(seed)?.RecordFullyGrown();
            }

            Add(points);
        }

        /// <summary>A plant dried out. The score never drops, so this only counts.</summary>
        public void RecordLost() => PlantsLost++;

        /// <summary>
        /// Adds a raw award.
        /// Negative points are refused rather than clamped per caller, so a future
        /// penalty has to be a deliberate change here.
        /// </summary>
        public void Add(int points)
        {
            if (points <= 0)
            {
                return;
            }

            Total += points;
            TotalChanged?.Invoke(Total);
        }

        /// <summary>Empties the tally for a soft reset. The scene reload path never calls it.</summary>
        public void Reset()
        {
            perSeed.Clear();
            PlantsPlanted = 0;
            TreesFullyGrown = 0;
            PlantsLost = 0;

            if (Total == 0)
            {
                return;
            }

            Total = 0;
            TotalChanged?.Invoke(Total);
        }

        // A null seed still scores. Only the per-seed row needs a type to hang on.
        private SeedTally TallyFor(SeedDefinition seed)
        {
            if (seed == null)
            {
                return null;
            }

            for (int i = 0; i < perSeed.Count; i++)
            {
                if (perSeed[i].Seed == seed)
                {
                    return perSeed[i];
                }
            }

            SeedTally created = new SeedTally(seed);
            perSeed.Add(created);
            return created;
        }
    }
}

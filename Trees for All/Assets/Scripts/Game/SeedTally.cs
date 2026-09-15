using Sogeti.Planting;

namespace Sogeti.Game
{
    /// <summary>
    /// What the player did with one seed type this round.
    /// The game over panel draws one row per instance. ScoreTally owns every
    /// instance, so the mutators are internal and no caller can invent a count.
    /// </summary>
    public class SeedTally
    {
        internal SeedTally(SeedDefinition seed)
        {
            Seed = seed;
        }

        public SeedDefinition Seed { get; }

        public int Planted { get; private set; }

        /// <summary>Plants of this type that reached the last growth stage.</summary>
        public int FullyGrown { get; private set; }

        internal void RecordPlanted() => Planted++;

        internal void RecordFullyGrown() => FullyGrown++;
    }
}

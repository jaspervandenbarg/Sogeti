using System.Runtime.CompilerServices;

// ScoreTally owns the per-seed counters, so SeedTally exposes internal mutators.
// Only the tally may raise a count, but the tests must still read one directly.
[assembly: InternalsVisibleTo("Sogeti.Planting.Tests")]

using System.Runtime.CompilerServices;

// The EditMode tests build SeedDefinition assets through an internal factory.
// Reflection on private fields would break silently on a rename.
[assembly: InternalsVisibleTo("Sogeti.Planting.Tests")]

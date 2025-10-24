using Xunit;

// Defines a shared test collection to disable parallelization for diagnostics tests only
[CollectionDefinition("Diagnostics", DisableParallelization = true)]
public class DiagnosticsCollectionDefinition
{
}

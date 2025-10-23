namespace OneTypePerFile;

public class FileViolation
{
    public required string FilePath { get; init; }
    public required List<TypeInfo> Types { get; init; }
}

public class TypeInfo
{
    public required string Name { get; init; }
    public required string Kind { get; init; }
    public required int Line { get; init; }
    public required string FullSource { get; init; }
    public required List<string> Usings { get; init; }
    public required string? Namespace { get; init; }
}

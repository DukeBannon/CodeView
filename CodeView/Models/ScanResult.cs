namespace CodeView.Models;

public sealed class ScanResult
{
    public required string RepositoryPath { get; init; }

    public required DateTimeOffset GeneratedAt { get; init; }

    public required IReadOnlyList<SourceFileInfo> Files { get; init; }

    public required IReadOnlyList<TodoItem> TodoItems { get; init; }

    public int TotalFiles => Files.Count;

    public int TotalLines => Files.Sum(file => file.LineCount);
}

namespace CodeView.Models;

public sealed class SourceFileInfo
{
    public required string FullPath { get; init; }

    public required string RelativePath { get; init; }

    public required string Extension { get; init; }

    public required IReadOnlyList<string> Lines { get; init; }

    public int LineCount => Lines.Count;
}

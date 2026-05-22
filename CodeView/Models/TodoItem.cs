namespace CodeView.Models;

public sealed class TodoItem
{
    public required string RelativePath { get; init; }

    public required int LineNumber { get; init; }

    public required string Keyword { get; init; }

    public required string Text { get; init; }
}

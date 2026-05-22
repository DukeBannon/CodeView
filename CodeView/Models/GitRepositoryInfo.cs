namespace CodeView.Models;

public sealed class GitRepositoryInfo
{
    public string? RootPath { get; init; }

    public string? BranchName { get; init; }

    public string? CommitHash { get; init; }

    public bool? IsDirty { get; init; }

    public bool IsRepository => !string.IsNullOrWhiteSpace(RootPath);

    public string DirtyStatus => IsDirty switch
    {
        true => "Dirty",
        false => "Clean",
        _ => "Unknown"
    };
}

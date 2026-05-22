namespace CodeView.Models;

public sealed class ListingOptions
{
    public bool IncludeTitlePage { get; set; } = true;

    public bool IncludeFileInventory { get; set; } = true;

    public bool IncludeTodoIndex { get; set; } = true;

    public bool IncludeSourceListing { get; set; } = true;

    public bool PageBreakBetweenFiles { get; set; } = true;

    public bool UseSyntaxHighlighting { get; set; } = true;

    public ListingStylePreset StylePreset { get; set; } = ListingStylePreset.CleanBinder;

    public LineWrapMode LineWrapMode { get; set; } = LineWrapMode.Wrap;

    public ListingOptions Clone()
    {
        return new ListingOptions
        {
            IncludeTitlePage = IncludeTitlePage,
            IncludeFileInventory = IncludeFileInventory,
            IncludeTodoIndex = IncludeTodoIndex,
            IncludeSourceListing = IncludeSourceListing,
            PageBreakBetweenFiles = PageBreakBetweenFiles,
            UseSyntaxHighlighting = UseSyntaxHighlighting,
            StylePreset = StylePreset,
            LineWrapMode = LineWrapMode
        };
    }
}

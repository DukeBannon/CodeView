using System.Text;
using CodeView.Models;

namespace CodeView.Output;

public sealed class TextListingGenerator
{
    public string Generate(ScanResult scanResult, ListingOptions options)
    {
        var text = new StringBuilder();

        if (options.IncludeTitlePage)
        {
            AppendTitlePage(text, scanResult);
        }

        AppendSectionTitle(text, "PROJECT SUMMARY");
        text.AppendLine($"Repository: {scanResult.RepositoryPath}");
        text.AppendLine($"Generated:  {scanResult.GeneratedAt:f}");
        text.AppendLine($"Files:      {scanResult.TotalFiles:N0}");
        text.AppendLine($"Lines:      {scanResult.TotalLines:N0}");
        AppendGitMetadata(text, scanResult);
        text.AppendLine();

        if (options.IncludeFileInventory)
        {
            AppendInventory(text, scanResult);
        }

        if (options.IncludeSourceListing)
        {
            AppendSourceListing(text, scanResult, options);
        }

        if (options.IncludeTodoIndex)
        {
            AppendTodoIndex(text, scanResult);
        }

        AppendSectionTitle(text, "CROSS-REFERENCE - PLANNED");
        text.AppendLine("Symbol and usage cross-reference generation is planned for a later prototype.");
        text.AppendLine();

        return text.ToString();
    }

    private static void AppendTitlePage(StringBuilder text, ScanResult scanResult)
    {
        text.AppendLine("CODEVIEW");
        text.AppendLine("Traditional Source Listing Generator");
        text.AppendLine(new string('=', 72));
        text.AppendLine($"Repository: {scanResult.RepositoryPath}");
        text.AppendLine($"Generated:  {scanResult.GeneratedAt:f}");
        text.AppendLine($"Files:      {scanResult.TotalFiles:N0}");
        text.AppendLine($"Lines:      {scanResult.TotalLines:N0}");
        AppendGitMetadata(text, scanResult);
        text.AppendLine();
        text.AppendLine();
    }

    private static void AppendInventory(StringBuilder text, ScanResult scanResult)
    {
        AppendSectionTitle(text, "FILE INVENTORY");
        text.AppendLine("SEQ   LINES      TYPE       RELATIVE PATH");
        text.AppendLine("----  ---------  ---------  --------------------------------------------------");

        for (var i = 0; i < scanResult.Files.Count; i++)
        {
            var file = scanResult.Files[i];
            text.AppendLine($"{i + 1:0000}  {file.LineCount,9:N0}  {file.Extension,-9}  {file.RelativePath}");
        }

        text.AppendLine();
    }

    private static void AppendSourceListing(StringBuilder text, ScanResult scanResult, ListingOptions options)
    {
        AppendSectionTitle(text, "SOURCE LISTING");

        for (var i = 0; i < scanResult.Files.Count; i++)
        {
            var file = scanResult.Files[i];

            if (i > 0 && options.PageBreakBetweenFiles)
            {
                text.AppendLine("\f");
            }

            text.AppendLine(new string('-', 96));
            text.AppendLine($"{i + 1:0000} | {file.RelativePath} | Type: {file.Extension} | Lines: {file.LineCount:N0}");
            text.AppendLine(new string('-', 96));

            for (var lineIndex = 0; lineIndex < file.Lines.Count; lineIndex++)
            {
                var renderedLines = GetRenderedSourceLines(file.Lines[lineIndex], options.LineWrapMode);

                for (var segmentIndex = 0; segmentIndex < renderedLines.Count; segmentIndex++)
                {
                    var lineNumber = segmentIndex == 0 ? $"{lineIndex + 1:000000}" : "  ....";
                    text.AppendLine($"{lineNumber}  {renderedLines[segmentIndex]}");
                }
            }

            text.AppendLine();
        }
    }

    private static void AppendTodoIndex(StringBuilder text, ScanResult scanResult)
    {
        AppendSectionTitle(text, "TODO/FIXME INDEX");

        if (scanResult.TodoItems.Count == 0)
        {
            text.AppendLine("No TODO, FIXME, HACK, BUG, or REVIEW markers were found.");
            text.AppendLine();
            return;
        }

        foreach (var item in scanResult.TodoItems)
        {
            text.AppendLine($"{item.Keyword,-6} {item.RelativePath}:{item.LineNumber:000000}  {item.Text}");
        }

        text.AppendLine();
    }

    private static void AppendSectionTitle(StringBuilder text, string title)
    {
        text.AppendLine(title);
        text.AppendLine(new string('=', title.Length));
    }

    private static void AppendGitMetadata(StringBuilder text, ScanResult scanResult)
    {
        if (!scanResult.GitInfo.IsRepository)
        {
            text.AppendLine("Git:        Not detected");
            return;
        }

        text.AppendLine($"Git Branch: {scanResult.GitInfo.BranchName ?? "Unknown"}");
        text.AppendLine($"Git Commit: {scanResult.GitInfo.CommitHash ?? "Unknown"}");
        text.AppendLine($"Git Status: {scanResult.GitInfo.DirtyStatus}");
    }

    private static IReadOnlyList<string> GetRenderedSourceLines(string line, LineWrapMode lineWrapMode)
    {
        const int continuationWidth = 116;

        if (lineWrapMode == LineWrapMode.Truncate && line.Length > continuationWidth)
        {
            return [line[..(continuationWidth - 3)] + "..."];
        }

        if (lineWrapMode != LineWrapMode.Continuation || line.Length <= continuationWidth)
        {
            return [line];
        }

        var lines = new List<string>();
        var remaining = line;
        var isContinuation = false;

        while (remaining.Length > continuationWidth)
        {
            lines.Add((isContinuation ? ">> " : string.Empty) + remaining[..continuationWidth]);
            remaining = remaining[continuationWidth..];
            isContinuation = true;
        }

        lines.Add(">> " + remaining);
        return lines;
    }
}

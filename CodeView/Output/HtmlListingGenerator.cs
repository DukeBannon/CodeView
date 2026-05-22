using System.Net;
using System.Text;
using CodeView.Models;

namespace CodeView.Output;

public sealed class HtmlListingGenerator
{
    public string Generate(ScanResult scanResult, ListingOptions options)
    {
        var html = new StringBuilder();

        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine("<title>CodeView Program Listing</title>");
        html.AppendLine("<style>");
        html.AppendLine(GetCss(options));
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        if (options.IncludeTitlePage)
        {
            AppendTitlePage(html, scanResult);
        }

        AppendTableOfContents(html, scanResult, options);
        AppendSummary(html, scanResult);

        if (options.IncludeFileInventory)
        {
            AppendFileInventory(html, scanResult);
        }

        if (options.IncludeSourceListing)
        {
            AppendSourceListing(html, scanResult, options);
        }

        if (options.IncludeTodoIndex)
        {
            AppendTodoIndex(html, scanResult);
        }

        AppendCrossReferencePlaceholder(html);
        AppendMetadata(html, scanResult);

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static string GetCss(ListingOptions options)
    {
        var fileBreak = options.PageBreakBetweenFiles ? "break-before: page; page-break-before: always;" : "";
        var styleCss = GetStyleCss(options.StylePreset);
        var lineWrapCss = GetLineWrapCss(options.LineWrapMode);

        return $$"""
            @page {
                size: letter;
                margin: 0.65in;
            }

            :root {
                color: #1d2329;
                background: #f4f1ea;
                font-family: "Segoe UI", Arial, sans-serif;
                font-size: 14px;
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }

            * {
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }

            body {
                margin: 0;
                padding: 28px;
                background: #f4f1ea;
            }

            .sheet {
                max-width: 1100px;
                margin: 0 auto 24px auto;
                padding: 42px 48px;
                background: #fffdf7;
                border: 1px solid #b8b1a3;
                box-shadow: 0 1px 4px rgba(0, 0, 0, 0.12);
            }

            h1, h2, h3 {
                margin: 0 0 14px 0;
                color: #111820;
                letter-spacing: 0;
            }

            h1 {
                font-size: 34px;
                text-transform: uppercase;
            }

            h2 {
                padding-bottom: 8px;
                border-bottom: 2px solid #313942;
                font-size: 22px;
            }

            .subtitle {
                margin-top: -8px;
                color: #4b5560;
                font-size: 18px;
            }

            .meta-grid {
                display: grid;
                grid-template-columns: 180px 1fr;
                gap: 8px 18px;
                margin-top: 28px;
            }

            .label {
                color: #4b5560;
                font-weight: 700;
                text-transform: uppercase;
                font-size: 12px;
            }

            table {
                width: 100%;
                border-collapse: collapse;
                margin-top: 16px;
            }

            a {
                color: #173f6f;
                text-decoration: none;
            }

            a:hover {
                text-decoration: underline;
            }

            .toc-list {
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
                gap: 8px 24px;
                margin-top: 18px;
                padding-left: 18px;
            }

            .toc-files {
                margin-top: 18px;
                columns: 2;
                column-gap: 34px;
                font-size: 12.5px;
            }

            .toc-files li {
                break-inside: avoid;
                margin-bottom: 4px;
            }

            th, td {
                padding: 7px 8px;
                border-bottom: 1px solid #d8d1c4;
                text-align: left;
                vertical-align: top;
            }

            th {
                background: #e7e1d6;
                color: #202832;
                font-size: 12px;
                text-transform: uppercase;
            }

            .file-section {
                {{fileBreak}}
            }

            .file-section.first-file {
                break-before: auto;
                page-break-before: auto;
            }

            .file-header {
                margin-top: 24px;
                padding: 12px 14px;
                border: 1px solid #2f3a45;
                background: #e8e4da;
                font-family: Consolas, "Courier New", monospace;
                font-weight: 700;
            }

            .source {
                margin: 0;
                border: 1px solid #cbc4b6;
                border-top: 0;
                background: #fffdf9;
                font-family: Consolas, "Courier New", monospace;
                font-size: 12.5px;
                line-height: 1.28;
                overflow-x: hidden;
            }

            .source-line {
                display: grid;
                grid-template-columns: 76px minmax(0, 1fr);
                min-height: 16px;
            }

            .line-number {
                padding-right: 12px;
                border-right: 1px solid #d8d1c4;
                color: #636b74;
                text-align: right;
                user-select: none;
            }

            .code {
                padding-left: 12px;
                min-width: 0;
                {{lineWrapCss}}
            }

            .hljs-keyword {
                color: #003f7d;
                font-weight: 700;
            }

            .hljs-string {
                color: #6c3f00;
            }

            .hljs-comment {
                color: #65746a;
                font-style: italic;
            }

            .hljs-number,
            .hljs-literal {
                color: #5a3f8c;
            }

            .hljs-tag {
                color: #1d5e6f;
                font-weight: 700;
            }

            .hljs-attr {
                color: #6d4b00;
            }

            .empty-state {
                margin-top: 16px;
                padding: 14px;
                border: 1px dashed #a9a195;
                color: #4b5560;
            }

            {{styleCss}}

            @media print {
                body {
                    padding: 0;
                }

                .sheet {
                    max-width: none;
                    margin: 0;
                    padding: 0;
                    box-shadow: none;
                    break-after: page;
                    page-break-after: always;
                }
            }
            """;
    }

    private static string GetStyleCss(ListingStylePreset stylePreset)
    {
        return stylePreset switch
        {
            ListingStylePreset.ClassicCompiler => """
                body { background: #ece8dc; }
                .sheet { background: #fffdf2; border-color: #57534a; box-shadow: none; }
                h1, h2, h3 { font-family: Consolas, "Courier New", monospace; text-transform: uppercase; }
                .file-header { background: #d8d2c2; border-color: #222; }
                .source { background: #fffdf2; border-color: #777166; }
                th { background: #d8d2c2; }
                """,
            ListingStylePreset.Greenbar => """
                body { background: #eef3ec; }
                .sheet { background: #fbfff8; border-color: #8fa58a; }
                .file-header { background: #dcebd6; border-color: #45633e; }
                .source { background: #fbfff8; border-color: #aabfa3; }
                .source-line:nth-child(6n+1),
                .source-line:nth-child(6n+2),
                .source-line:nth-child(6n+3) { background: #f4faef; }
                .source-line:nth-child(6n+4),
                .source-line:nth-child(6n+5),
                .source-line:nth-child(6n+6) { background: #ffffff; }
                th { background: #dcebd6; }
                """,
            ListingStylePreset.DenseReview => """
                body { padding: 18px; background: #f3f3ef; }
                .sheet { max-width: 1280px; padding: 28px 34px; background: #fffef8; }
                h1 { font-size: 28px; }
                h2 { font-size: 18px; }
                .source { font-size: 11px; line-height: 1.18; }
                .source-line { grid-template-columns: 66px minmax(0, 1fr); min-height: 13px; }
                .file-header { margin-top: 16px; padding: 8px 10px; }
                th, td { padding: 5px 6px; }
                """,
            _ => string.Empty
        };
    }

    private static string GetLineWrapCss(LineWrapMode lineWrapMode)
    {
        return lineWrapMode switch
        {
            LineWrapMode.Truncate => """
                display: block;
                white-space: pre;
                overflow: hidden;
                text-overflow: ellipsis;
                """,
            LineWrapMode.Continuation => """
                white-space: pre;
                overflow: hidden;
                """,
            _ => """
                white-space: pre-wrap;
                overflow-wrap: anywhere;
                word-break: break-word;
                """
        };
    }

    private static void AppendTitlePage(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\" id=\"title-page\">");
        html.AppendLine("<h1>CodeView</h1>");
        html.AppendLine("<div class=\"subtitle\">Traditional Source Listing Generator</div>");
        html.AppendLine("<div class=\"meta-grid\">");
        AppendMeta(html, "Repository", scanResult.RepositoryPath);
        AppendMeta(html, "Generated", scanResult.GeneratedAt.ToString("f"));
        AppendMeta(html, "Total Files", scanResult.TotalFiles.ToString("N0"));
        AppendMeta(html, "Total Lines", scanResult.TotalLines.ToString("N0"));
        AppendGitMetadata(html, scanResult);
        html.AppendLine("</div>");
        html.AppendLine("</section>");
    }

    private static void AppendTableOfContents(StringBuilder html, ScanResult scanResult, ListingOptions options)
    {
        html.AppendLine("<section class=\"sheet\" id=\"table-of-contents\">");
        html.AppendLine("<h2>Table of Contents</h2>");
        html.AppendLine("<ol class=\"toc-list\">");

        if (options.IncludeTitlePage)
        {
            html.AppendLine("<li><a href=\"#title-page\">Title Page</a></li>");
        }

        html.AppendLine("<li><a href=\"#project-summary\">Project Summary</a></li>");

        if (options.IncludeFileInventory)
        {
            html.AppendLine("<li><a href=\"#file-inventory\">File Inventory</a></li>");
        }

        if (options.IncludeSourceListing)
        {
            html.AppendLine("<li><a href=\"#source-listing\">Source Listing</a></li>");
        }

        if (options.IncludeTodoIndex)
        {
            html.AppendLine("<li><a href=\"#todo-index\">TODO/FIXME Index</a></li>");
        }

        html.AppendLine("<li><a href=\"#cross-reference\">Cross-Reference - Planned</a></li>");
        html.AppendLine("<li><a href=\"#generation-metadata\">Generation Metadata</a></li>");
        html.AppendLine("</ol>");

        if (options.IncludeSourceListing && scanResult.Files.Count > 0)
        {
            html.AppendLine("<h3>Files</h3>");
            html.AppendLine("<ol class=\"toc-files\">");

            for (var i = 0; i < scanResult.Files.Count; i++)
            {
                var file = scanResult.Files[i];
                html.AppendLine($"<li><a href=\"#{GetFileAnchor(i)}\">{i + 1:0000} | {Encode(file.RelativePath)}</a></li>");
            }

            html.AppendLine("</ol>");
        }

        html.AppendLine("</section>");
    }

    private static void AppendSummary(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\" id=\"project-summary\">");
        html.AppendLine("<h2>Project Summary</h2>");
        html.AppendLine("<div class=\"meta-grid\">");
        AppendMeta(html, "Repository", scanResult.RepositoryPath);
        AppendMeta(html, "Files Scanned", scanResult.TotalFiles.ToString("N0"));
        AppendMeta(html, "Source Lines", scanResult.TotalLines.ToString("N0"));
        AppendMeta(html, "TODO/FIXME Items", scanResult.TodoItems.Count.ToString("N0"));
        AppendGitMetadata(html, scanResult);
        html.AppendLine("</div>");
        html.AppendLine("</section>");
    }

    private static void AppendFileInventory(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\" id=\"file-inventory\">");
        html.AppendLine("<h2>File Inventory</h2>");
        html.AppendLine("<table>");
        html.AppendLine("<thead><tr><th>Seq</th><th>Relative Path</th><th>Type</th><th>Lines</th></tr></thead>");
        html.AppendLine("<tbody>");

        for (var i = 0; i < scanResult.Files.Count; i++)
        {
            var file = scanResult.Files[i];
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{i + 1:0000}</td>");
            html.AppendLine($"<td><a href=\"#{GetFileAnchor(i)}\">{Encode(file.RelativePath)}</a></td>");
            html.AppendLine($"<td>{Encode(file.Extension)}</td>");
            html.AppendLine($"<td>{file.LineCount:N0}</td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("</section>");
    }

    private static void AppendSourceListing(StringBuilder html, ScanResult scanResult, ListingOptions options)
    {
        html.AppendLine("<section class=\"sheet\" id=\"source-listing\">");
        html.AppendLine("<h2>Source Listing</h2>");

        for (var i = 0; i < scanResult.Files.Count; i++)
        {
            var file = scanResult.Files[i];
            var fileClass = i == 0 ? "file-section first-file" : "file-section";
            html.AppendLine($"<article class=\"{fileClass}\" id=\"{GetFileAnchor(i)}\">");
            html.AppendLine("<div class=\"file-header\">");
            html.AppendLine($"{i + 1:0000} | {Encode(file.RelativePath)} | Type: {Encode(file.Extension)} | Lines: {file.LineCount:N0}");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"source\">");

            for (var lineIndex = 0; lineIndex < file.Lines.Count; lineIndex++)
            {
                var renderedLines = GetRenderedSourceLines(file.Lines[lineIndex], options.LineWrapMode);

                for (var segmentIndex = 0; segmentIndex < renderedLines.Count; segmentIndex++)
                {
                    var renderedLine = renderedLines[segmentIndex];
                    var lineNumber = segmentIndex == 0 ? $"{lineIndex + 1:000000}" : "  ....";

                    html.Append("<div class=\"source-line\"><span class=\"line-number\">");
                    html.Append(lineNumber);
                    html.Append("</span><span class=\"code\">");
                    html.Append(options.UseSyntaxHighlighting
                        ? SimpleSyntaxHighlighter.Highlight(renderedLine, file.Extension)
                        : Encode(renderedLine));
                    html.AppendLine("</span></div>");
                }
            }

            html.AppendLine("</div>");
            html.AppendLine("</article>");
        }

        html.AppendLine("</section>");
    }

    private static void AppendTodoIndex(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\" id=\"todo-index\">");
        html.AppendLine("<h2>TODO/FIXME Index</h2>");

        if (scanResult.TodoItems.Count == 0)
        {
            html.AppendLine("<div class=\"empty-state\">No TODO, FIXME, HACK, BUG, or REVIEW markers were found.</div>");
            html.AppendLine("</section>");
            return;
        }

        html.AppendLine("<table>");
        html.AppendLine("<thead><tr><th>Keyword</th><th>File</th><th>Line</th><th>Text</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var item in scanResult.TodoItems)
        {
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{Encode(item.Keyword)}</td>");
            var fileIndex = scanResult.Files.ToList().FindIndex(file => string.Equals(file.RelativePath, item.RelativePath, StringComparison.OrdinalIgnoreCase));
            var fileLink = fileIndex >= 0
                ? $"<a href=\"#{GetFileAnchor(fileIndex)}\">{Encode(item.RelativePath)}</a>"
                : Encode(item.RelativePath);
            html.AppendLine($"<td>{fileLink}</td>");
            html.AppendLine($"<td>{item.LineNumber:000000}</td>");
            html.AppendLine($"<td><code>{Encode(item.Text)}</code></td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("</section>");
    }

    private static void AppendCrossReferencePlaceholder(StringBuilder html)
    {
        html.AppendLine("<section class=\"sheet\" id=\"cross-reference\">");
        html.AppendLine("<h2>Cross-Reference - Planned</h2>");
        html.AppendLine("<div class=\"empty-state\">Symbol and usage cross-reference generation is planned for a later prototype.</div>");
        html.AppendLine("</section>");
    }

    private static void AppendMetadata(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\" id=\"generation-metadata\">");
        html.AppendLine("<h2>Generation Metadata</h2>");
        html.AppendLine("<div class=\"meta-grid\">");
        AppendMeta(html, "Generator", "CodeView Prototype 1");
        AppendMeta(html, "Generated", scanResult.GeneratedAt.ToString("O"));
        AppendMeta(html, "Machine", Environment.MachineName);
        AppendMeta(html, "User", Environment.UserName);
        AppendGitMetadata(html, scanResult);
        html.AppendLine("</div>");
        html.AppendLine("</section>");
    }

    private static void AppendMeta(StringBuilder html, string label, string value)
    {
        html.AppendLine($"<div class=\"label\">{Encode(label)}</div><div>{Encode(value)}</div>");
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    private static string GetFileAnchor(int index)
    {
        return $"file-{index + 1:0000}";
    }

    private static IReadOnlyList<string> GetRenderedSourceLines(string line, LineWrapMode lineWrapMode)
    {
        const int continuationWidth = 116;

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

    private static void AppendGitMetadata(StringBuilder html, ScanResult scanResult)
    {
        if (!scanResult.GitInfo.IsRepository)
        {
            AppendMeta(html, "Git", "Not detected");
            return;
        }

        AppendMeta(html, "Git Branch", scanResult.GitInfo.BranchName ?? "Unknown");
        AppendMeta(html, "Git Commit", scanResult.GitInfo.CommitHash ?? "Unknown");
        AppendMeta(html, "Git Status", scanResult.GitInfo.DirtyStatus);
    }
}

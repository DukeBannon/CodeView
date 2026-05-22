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
                white-space: pre-wrap;
                overflow-wrap: anywhere;
                word-break: break-word;
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

            @media print {
                body {
                    padding: 0;
                    background: white;
                }

                .sheet {
                    max-width: none;
                    margin: 0;
                    padding: 0;
                    border: 0;
                    box-shadow: none;
                    break-after: page;
                    page-break-after: always;
                }
            }
            """;
    }

    private static void AppendTitlePage(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\">");
        html.AppendLine("<h1>CodeView</h1>");
        html.AppendLine("<div class=\"subtitle\">Traditional Source Listing Generator</div>");
        html.AppendLine("<div class=\"meta-grid\">");
        AppendMeta(html, "Repository", scanResult.RepositoryPath);
        AppendMeta(html, "Generated", scanResult.GeneratedAt.ToString("f"));
        AppendMeta(html, "Total Files", scanResult.TotalFiles.ToString("N0"));
        AppendMeta(html, "Total Lines", scanResult.TotalLines.ToString("N0"));
        html.AppendLine("</div>");
        html.AppendLine("</section>");
    }

    private static void AppendSummary(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\">");
        html.AppendLine("<h2>Project Summary</h2>");
        html.AppendLine("<div class=\"meta-grid\">");
        AppendMeta(html, "Repository", scanResult.RepositoryPath);
        AppendMeta(html, "Files Scanned", scanResult.TotalFiles.ToString("N0"));
        AppendMeta(html, "Source Lines", scanResult.TotalLines.ToString("N0"));
        AppendMeta(html, "TODO/FIXME Items", scanResult.TodoItems.Count.ToString("N0"));
        html.AppendLine("</div>");
        html.AppendLine("</section>");
    }

    private static void AppendFileInventory(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\">");
        html.AppendLine("<h2>File Inventory</h2>");
        html.AppendLine("<table>");
        html.AppendLine("<thead><tr><th>Seq</th><th>Relative Path</th><th>Type</th><th>Lines</th></tr></thead>");
        html.AppendLine("<tbody>");

        for (var i = 0; i < scanResult.Files.Count; i++)
        {
            var file = scanResult.Files[i];
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{i + 1:0000}</td>");
            html.AppendLine($"<td>{Encode(file.RelativePath)}</td>");
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
        html.AppendLine("<section class=\"sheet\">");
        html.AppendLine("<h2>Source Listing</h2>");

        for (var i = 0; i < scanResult.Files.Count; i++)
        {
            var file = scanResult.Files[i];
            var fileClass = i == 0 ? "file-section first-file" : "file-section";
            html.AppendLine($"<article class=\"{fileClass}\">");
            html.AppendLine("<div class=\"file-header\">");
            html.AppendLine($"{i + 1:0000} | {Encode(file.RelativePath)} | Type: {Encode(file.Extension)} | Lines: {file.LineCount:N0}");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"source\">");

            for (var lineIndex = 0; lineIndex < file.Lines.Count; lineIndex++)
            {
                html.Append("<div class=\"source-line\"><span class=\"line-number\">");
                html.Append($"{lineIndex + 1:000000}");
                html.Append("</span><span class=\"code\">");
                html.Append(options.UseSyntaxHighlighting
                    ? SimpleSyntaxHighlighter.Highlight(file.Lines[lineIndex], file.Extension)
                    : Encode(file.Lines[lineIndex]));
                html.AppendLine("</span></div>");
            }

            html.AppendLine("</div>");
            html.AppendLine("</article>");
        }

        html.AppendLine("</section>");
    }

    private static void AppendTodoIndex(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\">");
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
            html.AppendLine($"<td>{Encode(item.RelativePath)}</td>");
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
        html.AppendLine("<section class=\"sheet\">");
        html.AppendLine("<h2>Cross-Reference - Planned</h2>");
        html.AppendLine("<div class=\"empty-state\">Symbol and usage cross-reference generation is planned for a later prototype.</div>");
        html.AppendLine("</section>");
    }

    private static void AppendMetadata(StringBuilder html, ScanResult scanResult)
    {
        html.AppendLine("<section class=\"sheet\">");
        html.AppendLine("<h2>Generation Metadata</h2>");
        html.AppendLine("<div class=\"meta-grid\">");
        AppendMeta(html, "Generator", "CodeView Prototype 1");
        AppendMeta(html, "Generated", scanResult.GeneratedAt.ToString("O"));
        AppendMeta(html, "Machine", Environment.MachineName);
        AppendMeta(html, "User", Environment.UserName);
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
}

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeView.Output;

public static partial class SimpleSyntaxHighlighter
{
    private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "get", "global", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
        "out", "override", "params", "partial", "private", "protected", "public", "readonly",
        "record", "ref", "return", "sbyte", "sealed", "set", "short", "sizeof", "stackalloc",
        "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void",
        "volatile", "while", "where", "yield"
    };

    private static readonly HashSet<string> PascalKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "array", "as", "asm", "begin", "case", "class", "const", "constructor",
        "destructor", "div", "do", "downto", "else", "end", "except", "exports", "file",
        "finalization", "finally", "for", "function", "goto", "if", "implementation", "in",
        "inherited", "initialization", "inline", "interface", "is", "label", "library", "mod",
        "nil", "not", "object", "of", "or", "packed", "procedure", "program", "property",
        "raise", "record", "repeat", "resourcestring", "set", "shl", "shr", "then", "threadvar",
        "to", "try", "type", "unit", "until", "uses", "var", "while", "with", "xor"
    };

    private static readonly HashSet<string> CobolKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "ACCEPT", "ADD", "CALL", "CLOSE", "COMPUTE", "CONFIGURATION", "CONTINUE", "DATA",
        "DELETE", "DISPLAY", "DIVIDE", "DIVISION", "ELSE", "END", "END-IF", "ENVIRONMENT",
        "EVALUATE", "EXIT", "FD", "FILE", "FROM", "GO", "GOBACK", "IDENTIFICATION", "IF",
        "INITIALIZE", "INPUT-OUTPUT", "INSPECT", "INTO", "LINKAGE", "MOVE", "MULTIPLY",
        "OPEN", "PARAGRAPH", "PERFORM", "PIC", "PICTURE", "PROCEDURE", "PROGRAM-ID", "READ",
        "REDEFINES", "RETURN", "REWRITE", "SECTION", "SELECT", "SET", "STOP", "STRING",
        "SUBTRACT", "THEN", "TO", "UNTIL", "VALUE", "VARYING", "WHEN", "WORKING-STORAGE",
        "WRITE"
    };

    private static readonly HashSet<string> AssemblyKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "aaa", "aad", "aam", "aas", "adc", "add", "and", "call", "cmp", "db", "dd", "dec",
        "div", "dq", "dw", "end", "equ", "extern", "global", "idiv", "imul", "in", "inc",
        "int", "ja", "jae", "jb", "jbe", "jc", "je", "jg", "jge", "jl", "jle", "jmp",
        "jne", "jnz", "jz", "lea", "loop", "mov", "mul", "neg", "nop", "not", "or", "org",
        "out", "pop", "proc", "push", "ret", "section", "segment", "shl", "shr", "sub",
        "test", "xor"
    };

    public static string Highlight(string line, string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cs" => HighlightCSharp(line),
            ".xaml" or ".xml" or ".config" or ".csproj" => HighlightXml(line),
            ".json" => HighlightJson(line),
            ".sql" => HighlightSql(line),
            ".cbl" or ".cob" or ".cpy" => HighlightCobol(line),
            ".pas" or ".dpr" => HighlightPascal(line),
            ".dfm" => HighlightDfm(line),
            ".asm" or ".s" or ".inc" or ".mac" => HighlightAssembly(line),
            _ => Encode(line)
        };
    }

    private static string HighlightCSharp(string line)
    {
        var commentIndex = line.IndexOf("//", StringComparison.Ordinal);
        var code = commentIndex >= 0 ? line[..commentIndex] : line;
        var comment = commentIndex >= 0 ? line[commentIndex..] : null;

        return HighlightTokens(code, CSharpTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith('"'))
            {
                return Span("hljs-string", value);
            }

            if (char.IsDigit(value[0]))
            {
                return Span("hljs-number", value);
            }

            return CSharpKeywords.Contains(value) ? Span("hljs-keyword", value) : Encode(value);
        }) + (comment is null ? string.Empty : Span("hljs-comment", comment));
    }

    private static string HighlightXml(string line)
    {
        return HighlightTokens(line, XmlTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith('<'))
            {
                return Span("hljs-tag", value);
            }

            if (value.StartsWith('"'))
            {
                return Span("hljs-string", value);
            }

            if (value.EndsWith('='))
            {
                return Span("hljs-attr", value[..^1]) + "=";
            }

            return Encode(value);
        });
    }

    private static string HighlightJson(string line)
    {
        return HighlightTokens(line, JsonTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith('"'))
            {
                var after = line[(match.Index + match.Length)..].TrimStart();
                return after.StartsWith(':') ? Span("hljs-attr", value) : Span("hljs-string", value);
            }

            if (char.IsDigit(value[0]) || value[0] == '-')
            {
                return Span("hljs-number", value);
            }

            return Span("hljs-literal", value);
        });
    }

    private static string HighlightSql(string line)
    {
        return HighlightTokens(line, SqlTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith('\''))
            {
                return Span("hljs-string", value);
            }

            if (char.IsDigit(value[0]))
            {
                return Span("hljs-number", value);
            }

            return Span("hljs-keyword", value);
        });
    }

    private static string HighlightCobol(string line)
    {
        if (line.Length > 6 && (line[6] == '*' || line[6] == '/'))
        {
            return Span("hljs-comment", line);
        }

        return HighlightTokens(line, CobolTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith("*>", StringComparison.Ordinal) || value.StartsWith('*'))
            {
                return Span("hljs-comment", value);
            }

            if (value.StartsWith('\'') || value.StartsWith('"'))
            {
                return Span("hljs-string", value);
            }

            if (char.IsDigit(value[0]))
            {
                return Span("hljs-number", value);
            }

            return CobolKeywords.Contains(value) ? Span("hljs-keyword", value) : Encode(value);
        });
    }

    private static string HighlightPascal(string line)
    {
        return HighlightTokens(line, PascalTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith('{') || value.StartsWith("(*", StringComparison.Ordinal) || value.StartsWith("//", StringComparison.Ordinal))
            {
                return Span("hljs-comment", value);
            }

            if (value.StartsWith('\''))
            {
                return Span("hljs-string", value);
            }

            if (char.IsDigit(value[0]) || value.StartsWith('$'))
            {
                return Span("hljs-number", value);
            }

            return PascalKeywords.Contains(value) ? Span("hljs-keyword", value) : Encode(value);
        });
    }

    private static string HighlightDfm(string line)
    {
        return HighlightTokens(line, DfmTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith('\''))
            {
                return Span("hljs-string", value);
            }

            if (char.IsDigit(value[0]) || value[0] == '-')
            {
                return Span("hljs-number", value);
            }

            if (value.Equals("object", StringComparison.OrdinalIgnoreCase)
                || value.Equals("inherited", StringComparison.OrdinalIgnoreCase)
                || value.Equals("inline", StringComparison.OrdinalIgnoreCase)
                || value.Equals("end", StringComparison.OrdinalIgnoreCase))
            {
                return Span("hljs-keyword", value);
            }

            return Encode(value);
        });
    }

    private static string HighlightAssembly(string line)
    {
        var commentIndex = line.IndexOfAny([';', '#']);
        var code = commentIndex >= 0 ? line[..commentIndex] : line;
        var comment = commentIndex >= 0 ? line[commentIndex..] : null;

        return HighlightTokens(code, AssemblyTokenRegex(), match =>
        {
            var value = match.Value;

            if (value.StartsWith('\'') || value.StartsWith('"'))
            {
                return Span("hljs-string", value);
            }

            if (char.IsDigit(value[0]) || value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || value.EndsWith('h'))
            {
                return Span("hljs-number", value);
            }

            if (value.EndsWith(':'))
            {
                return Span("hljs-title", value);
            }

            return AssemblyKeywords.Contains(value) ? Span("hljs-keyword", value) : Encode(value);
        }) + (comment is null ? string.Empty : Span("hljs-comment", comment));
    }

    private static string HighlightTokens(string line, Regex regex, Func<Match, string> formatMatch)
    {
        var html = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in regex.Matches(line))
        {
            if (match.Index > lastIndex)
            {
                html.Append(Encode(line[lastIndex..match.Index]));
            }

            html.Append(formatMatch(match));
            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < line.Length)
        {
            html.Append(Encode(line[lastIndex..]));
        }

        return html.ToString();
    }

    private static string Span(string className, string value)
    {
        return $"<span class=\"{className}\">{Encode(value)}</span>";
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    [GeneratedRegex("\"(?:\\\\.|[^\"\\\\])*\"|\\b\\d+(?:\\.\\d+)?\\b|\\b[A-Za-z_][A-Za-z0-9_]*\\b")]
    private static partial Regex CSharpTokenRegex();

    [GeneratedRegex("</?[A-Za-z_][A-Za-z0-9_.:-]*|\\b[A-Za-z_][A-Za-z0-9_.:-]*(?==)=|\"[^\"]*\"")]
    private static partial Regex XmlTokenRegex();

    [GeneratedRegex("\"(?:\\\\.|[^\"\\\\])*\"|-?\\b\\d+(?:\\.\\d+)?\\b|\\btrue\\b|\\bfalse\\b|\\bnull\\b", RegexOptions.IgnoreCase)]
    private static partial Regex JsonTokenRegex();

    [GeneratedRegex("'(?:''|[^'])*'|\\b\\d+(?:\\.\\d+)?\\b|\\b(SELECT|FROM|WHERE|JOIN|INNER|LEFT|RIGHT|FULL|OUTER|ON|INSERT|INTO|UPDATE|DELETE|CREATE|ALTER|DROP|TABLE|VIEW|PROCEDURE|FUNCTION|DECLARE|SET|AS|AND|OR|NOT|NULL|IS|IN|EXISTS|ORDER|BY|GROUP|HAVING|UNION|ALL|DISTINCT|TOP|VALUES)\\b", RegexOptions.IgnoreCase)]
    private static partial Regex SqlTokenRegex();

    [GeneratedRegex("\"(?:\"\"|[^\"])*\"|'(?:''|[^'])*'|\\*>.*$|\\*.*$|\\b\\d+(?:\\.\\d+)?\\b|\\b[A-Z][A-Z0-9-]*\\b", RegexOptions.IgnoreCase)]
    private static partial Regex CobolTokenRegex();

    [GeneratedRegex("'(?:''|[^'])*'|\\{[^}]*\\}|\\(\\*.*?\\*\\)|//.*$|\\$[0-9A-Fa-f]+|\\b\\d+(?:\\.\\d+)?\\b|\\b[A-Za-z_][A-Za-z0-9_]*\\b")]
    private static partial Regex PascalTokenRegex();

    [GeneratedRegex("'(?:''|[^'])*'|-?\\b\\d+(?:\\.\\d+)?\\b|\\b[A-Za-z_][A-Za-z0-9_]*\\b")]
    private static partial Regex DfmTokenRegex();

    [GeneratedRegex("\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'|0x[0-9A-Fa-f]+|\\b[0-9A-Fa-f]+h\\b|\\b\\d+\\b|\\b[A-Za-z_.$@][A-Za-z0-9_.$@]*:|\\b[A-Za-z.][A-Za-z0-9_.]*\\b")]
    private static partial Regex AssemblyTokenRegex();
}

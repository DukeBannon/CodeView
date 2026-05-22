# CodeView

CodeView is a local Windows desktop utility for generating traditional formatted program listings from source-code repositories. The style is inspired by old compiler and mainframe listings, paired with a modern WPF preview pane.

## Prototype 1 Scope

- Select a repository folder for the current session.
- Recursively scan supported source and text files.
- Exclude common build, IDE, dependency, and generated/user files.
- Show a file inventory with relative paths and line counts.
- Generate an HTML preview with a title page, project summary, inventory, source listing, TODO/FIXME index, cross-reference placeholder, and generation metadata.
- Optionally apply local Highlight.js-style syntax highlighting for common prototype file types, including C#, XML/XAML, JSON, SQL, COBOL, Pascal/Delphi, DFM, and assembly.
- Export the listing as HTML or plain text.
- Use WebView2 for the preview pane.

## How To Run

1. Install .NET 8 SDK and Visual Studio 2022 with the .NET desktop workload.
2. Open `CodeView.sln` in Visual Studio 2022.
3. Restore NuGet packages if prompted.
4. Build and run the `CodeView` WPF application.

From a terminal:

```powershell
dotnet restore
dotnet build
dotnet run --project CodeView\CodeView.csproj
```

## Current Limitations

- Repository path is stored in memory only.
- Export supports HTML and plain text only.
- The table of contents is not clickable yet.
- Cross-reference output is a planned placeholder only.
- Source file ordering is simple relative-path sorting.
- Very large repositories may take time to scan and render in the prototype UI.

## Planned Features

- PDF export
- Print button
- Clickable table of contents
- Basic cross-reference
- C# Roslyn-based cross-reference
- COBOL/Pascal symbol scanning
- Greenbar and Classic Compiler output styles

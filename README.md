# CodeView

CodeView is a local Windows desktop utility for generating traditional formatted program listings from source-code repositories. The style is inspired by old compiler and mainframe listings, paired with a modern WPF preview pane.

## Prototype Scope

- Select a repository folder for the current session.
- Recursively scan supported source and text files.
- Exclude common build, IDE, dependency, and generated/user files.
- Show a file inventory with relative paths and line counts.
- Generate an HTML preview with a title page, clickable table of contents, project summary, inventory, source listing, TODO/FIXME index, cross-reference placeholder, and generation metadata.
- Optionally apply local Highlight.js-style syntax highlighting for common prototype file types, including C#, XML/XAML, JSON, SQL, COBOL, Pascal/Delphi, DFM, and assembly.
- Include Git branch, commit, and dirty/clean status when the selected repository is a Git repo.
- Export the listing as HTML, PDF, or plain text.
- Print the current preview from WebView2.
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
- Cross-reference output is a planned placeholder only.
- Source file ordering is simple relative-path sorting.
- Very large repositories may take time to scan and render in the prototype UI.

## Planned Features

- Basic cross-reference
- C# Roslyn-based cross-reference
- COBOL/Pascal symbol scanning
- Greenbar and Classic Compiler output styles

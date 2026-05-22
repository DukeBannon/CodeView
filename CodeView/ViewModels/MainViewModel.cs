using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CodeView.Models;
using CodeView.Output;
using CodeView.Scanning;
using CodeView.Services;

namespace CodeView.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly RepositoryScanner _scanner = new();
    private readonly HtmlListingGenerator _htmlGenerator = new();
    private readonly TextListingGenerator _textGenerator = new();
    private readonly IUserDialogService _dialogService;
    private ScanResult? _scanResult;
    private string? _selectedRepositoryPath;
    private string? _previewHtml;
    private string _statusMessage = "Choose a repository folder to begin.";
    private bool _isBusy;
    private bool _includeTitlePage = true;
    private bool _includeFileInventory = true;
    private bool _includeTodoIndex = true;
    private bool _includeSourceListing = true;
    private bool _pageBreakBetweenFiles = true;
    private bool _useSyntaxHighlighting = true;

    public MainViewModel()
        : this(new UserDialogService())
    {
    }

    public MainViewModel(IUserDialogService dialogService)
    {
        _dialogService = dialogService;
        OpenRepoCommand = new RelayCommand(OpenRepository);
        ScanRepoCommand = new AsyncRelayCommand(ScanRepositoryAsync, CanScan);
        GeneratePreviewCommand = new RelayCommand(GeneratePreview, HasScanResult);
        ExportHtmlCommand = new RelayCommand(ExportHtml, HasScanResult);
        ExportTextCommand = new RelayCommand(ExportText, HasScanResult);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<SourceFileInfo> Files { get; } = [];

    public ICommand OpenRepoCommand { get; }

    public ICommand ScanRepoCommand { get; }

    public ICommand GeneratePreviewCommand { get; }

    public ICommand ExportHtmlCommand { get; }

    public ICommand ExportTextCommand { get; }

    public string? SelectedRepositoryPath
    {
        get => _selectedRepositoryPath;
        private set
        {
            if (SetField(ref _selectedRepositoryPath, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string SelectedRepositoryDisplay => string.IsNullOrWhiteSpace(SelectedRepositoryPath)
        ? "No repository selected"
        : SelectedRepositoryPath;

    public string? PreviewHtml
    {
        get => _previewHtml;
        private set
        {
            _previewHtml = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetField(ref _isBusy, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public bool IncludeTitlePage
    {
        get => _includeTitlePage;
        set => SetOption(ref _includeTitlePage, value);
    }

    public bool IncludeFileInventory
    {
        get => _includeFileInventory;
        set => SetOption(ref _includeFileInventory, value);
    }

    public bool IncludeTodoIndex
    {
        get => _includeTodoIndex;
        set => SetOption(ref _includeTodoIndex, value);
    }

    public bool IncludeSourceListing
    {
        get => _includeSourceListing;
        set => SetOption(ref _includeSourceListing, value);
    }

    public bool PageBreakBetweenFiles
    {
        get => _pageBreakBetweenFiles;
        set => SetOption(ref _pageBreakBetweenFiles, value);
    }

    public bool UseSyntaxHighlighting
    {
        get => _useSyntaxHighlighting;
        set => SetOption(ref _useSyntaxHighlighting, value);
    }

    public int TotalFiles => _scanResult?.TotalFiles ?? 0;

    public int TotalLines => _scanResult?.TotalLines ?? 0;

    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    private void OpenRepository()
    {
        var selectedPath = _dialogService.ChooseRepositoryFolder(SelectedRepositoryPath);
        if (selectedPath is null)
        {
            return;
        }

        SelectedRepositoryPath = selectedPath;
        StatusMessage = "Repository selected. Scan the repo to build the file inventory.";
        OnPropertyChanged(nameof(SelectedRepositoryDisplay));
    }

    private async Task ScanRepositoryAsync()
    {
        if (SelectedRepositoryPath is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Scanning repository...";

            _scanResult = await _scanner.ScanAsync(SelectedRepositoryPath);
            Files.Clear();

            foreach (var file in _scanResult.Files)
            {
                Files.Add(file);
            }

            OnPropertyChanged(nameof(TotalFiles));
            OnPropertyChanged(nameof(TotalLines));
            PreviewHtml = BuildScanCompleteHtml(TotalFiles, TotalLines);
            StatusMessage = $"Scan complete: {TotalFiles:N0} files, {TotalLines:N0} lines. Click Generate Preview to render the listing.";
        }
        catch (Exception ex)
        {
            ShowError("Scan failed", ex);
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private void GeneratePreview()
    {
        if (_scanResult is null)
        {
            StatusMessage = "Scan a repository before generating a preview.";
            return;
        }

        try
        {
            StatusMessage = "Generating preview...";
            PreviewHtml = _htmlGenerator.Generate(_scanResult, GetOptions());
            StatusMessage = $"Preview generated at {DateTime.Now:t}.";
        }
        catch (Exception ex)
        {
            ShowError("Preview generation failed", ex);
        }
    }

    private void ExportHtml()
    {
        if (_scanResult is null)
        {
            return;
        }

        var fileName = _dialogService.ChooseHtmlExportPath(GetDefaultFileName("html"));
        if (fileName is null)
        {
            return;
        }

        try
        {
            var html = _htmlGenerator.Generate(_scanResult, GetOptions());
            File.WriteAllText(fileName, html);
            StatusMessage = $"HTML exported to {fileName}";
        }
        catch (Exception ex)
        {
            ShowError("HTML export failed", ex);
        }
    }

    private void ExportText()
    {
        if (_scanResult is null)
        {
            return;
        }

        var fileName = _dialogService.ChooseTextExportPath(GetDefaultFileName("txt"));
        if (fileName is null)
        {
            return;
        }

        try
        {
            var text = _textGenerator.Generate(_scanResult, GetOptions());
            File.WriteAllText(fileName, text);
            StatusMessage = $"Text exported to {fileName}";
        }
        catch (Exception ex)
        {
            ShowError("Text export failed", ex);
        }
    }

    private ListingOptions GetOptions()
    {
        return new ListingOptions
        {
            IncludeTitlePage = IncludeTitlePage,
            IncludeFileInventory = IncludeFileInventory,
            IncludeTodoIndex = IncludeTodoIndex,
            IncludeSourceListing = IncludeSourceListing,
            PageBreakBetweenFiles = PageBreakBetweenFiles,
            UseSyntaxHighlighting = UseSyntaxHighlighting
        };
    }

    private string GetDefaultFileName(string extension)
    {
        var repoName = Path.GetFileName(SelectedRepositoryPath?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(repoName))
        {
            repoName = "CodeView";
        }

        return $"{repoName}-listing.{extension}";
    }

    private bool CanScan()
    {
        return !IsBusy && Directory.Exists(SelectedRepositoryPath);
    }

    private bool HasScanResult()
    {
        return !IsBusy && _scanResult is not null;
    }

    private void SetOption(ref bool field, bool value, [CallerMemberName] string? propertyName = null)
    {
        if (SetField(ref field, value, propertyName) && _scanResult is not null)
        {
            GeneratePreview();
        }
    }

    private void ShowError(string title, Exception exception)
    {
        StatusMessage = $"{title}: {exception.Message}";
        PreviewHtml = BuildErrorHtml(title, exception.Message);
        _dialogService.ShowError(title, exception.Message);
    }

    private static string BuildErrorHtml(string title, string message)
    {
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: "Segoe UI", Arial, sans-serif; padding: 32px; background: #fffdf7; color: #1d2329; }
                    .error { border: 1px solid #8a2c2c; background: #fff4f4; padding: 18px; }
                    h1 { margin-top: 0; color: #8a2c2c; }
                </style>
            </head>
            <body>
                <div class="error">
                    <h1>{{System.Net.WebUtility.HtmlEncode(title)}}</h1>
                    <p>{{System.Net.WebUtility.HtmlEncode(message)}}</p>
                </div>
            </body>
            </html>
            """;
    }

    private static string BuildScanCompleteHtml(int totalFiles, int totalLines)
    {
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: "Segoe UI", Arial, sans-serif; padding: 32px; background: #fffdf7; color: #1d2329; }
                    .panel { max-width: 760px; border: 1px solid #b8b1a3; background: #f8f5ee; padding: 28px; }
                    h1 { margin-top: 0; }
                    .metric { font-weight: 700; }
                </style>
            </head>
            <body>
                <div class="panel">
                    <h1>Scan Complete</h1>
                    <p><span class="metric">{{totalFiles:N0}}</span> files and <span class="metric">{{totalLines:N0}}</span> lines are ready.</p>
                    <p>Click <strong>Generate Preview</strong> to render the formatted program listing.</p>
                </div>
            </body>
            </html>
            """;
    }

    private void RaiseCommandStates()
    {
        ((RelayCommand)OpenRepoCommand).RaiseCanExecuteChanged();
        ((AsyncRelayCommand)ScanRepoCommand).RaiseCanExecuteChanged();
        ((RelayCommand)GeneratePreviewCommand).RaiseCanExecuteChanged();
        ((RelayCommand)ExportHtmlCommand).RaiseCanExecuteChanged();
        ((RelayCommand)ExportTextCommand).RaiseCanExecuteChanged();
        OnPropertyChanged(nameof(SelectedRepositoryDisplay));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

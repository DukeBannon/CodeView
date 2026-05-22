using System.ComponentModel;
using System.Windows;
using CodeView.ViewModels;
using Microsoft.Win32;

namespace CodeView.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private bool _webViewReady;
    private string? _pendingHtml;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await PreviewBrowser.EnsureCoreWebView2Async();
            _webViewReady = true;
            NavigateToHtml(_pendingHtml ?? GetWelcomeHtml());
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"WebView2 preview could not initialize: {ex.Message}",
                "Preview initialization failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.PreviewHtml))
        {
            return;
        }

        NavigateToHtml(_viewModel.PreviewHtml);
    }

    private void NavigateToHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return;
        }

        if (!_webViewReady)
        {
            _pendingHtml = html;
            return;
        }

        PreviewBrowser.NavigateToString(html);
    }

    private async void OnPrintPreviewClick(object sender, RoutedEventArgs e)
    {
        if (!_webViewReady || PreviewBrowser.CoreWebView2 is null)
        {
            _viewModel.SetStatusMessage("Preview is not ready to print yet.");
            return;
        }

        try
        {
            await PreviewBrowser.CoreWebView2.ExecuteScriptAsync("window.print();");
            _viewModel.SetStatusMessage("Print dialog opened.");
        }
        catch (Exception ex)
        {
            _viewModel.SetStatusMessage($"Print failed: {ex.Message}");
            System.Windows.MessageBox.Show(
                ex.Message,
                "Print failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void OnExportPdfClick(object sender, RoutedEventArgs e)
    {
        if (!_webViewReady || PreviewBrowser.CoreWebView2 is null)
        {
            _viewModel.SetStatusMessage("Preview is not ready to export as PDF yet.");
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Export PDF Listing",
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
            FileName = "CodeView-listing.pdf"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var success = await PreviewBrowser.CoreWebView2.PrintToPdfAsync(dialog.FileName);
            _viewModel.SetStatusMessage(success
                ? $"PDF exported to {dialog.FileName}"
                : "PDF export was canceled or did not complete.");
        }
        catch (Exception ex)
        {
            _viewModel.SetStatusMessage($"PDF export failed: {ex.Message}");
            System.Windows.MessageBox.Show(
                ex.Message,
                "PDF export failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static string GetWelcomeHtml()
    {
        return """
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <style>
                    body {
                        margin: 0;
                        padding: 32px;
                        background: #fffdf7;
                        color: #1d2329;
                        font-family: "Segoe UI", Arial, sans-serif;
                    }
                    .panel {
                        max-width: 760px;
                        border: 1px solid #b8b1a3;
                        background: #f8f5ee;
                        padding: 28px;
                    }
                    h1 {
                        margin-top: 0;
                    }
                </style>
            </head>
            <body>
                <div class="panel">
                    <h1>CodeView Preview</h1>
                    <p>Select a repository, scan it, and generate a traditional source listing preview.</p>
                </div>
            </body>
            </html>
            """;
    }
}

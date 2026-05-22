using System.ComponentModel;
using System.Windows;
using CodeView.ViewModels;

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

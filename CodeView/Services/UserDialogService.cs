using Forms = System.Windows.Forms;
using System.IO;

namespace CodeView.Services;

public sealed class UserDialogService : IUserDialogService
{
    public string? ChooseRepositoryFolder(string? currentPath)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select a source-code repository folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
            SelectedPath = Directory.Exists(currentPath) ? currentPath : string.Empty
        };

        return dialog.ShowDialog() == Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }

    public string? ChooseHtmlExportPath(string suggestedFileName)
    {
        return ChooseExportPath("Export HTML Listing", "HTML files (*.html)|*.html|All files (*.*)|*.*", suggestedFileName);
    }

    public string? ChooseTextExportPath(string suggestedFileName)
    {
        return ChooseExportPath("Export Text Listing", "Text files (*.txt)|*.txt|All files (*.*)|*.*", suggestedFileName);
    }

    public void ShowError(string title, string message)
    {
        System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }

    private static string? ChooseExportPath(string title, string filter, string suggestedFileName)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = title,
            Filter = filter,
            FileName = suggestedFileName
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

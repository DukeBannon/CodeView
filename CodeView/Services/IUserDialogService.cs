namespace CodeView.Services;

public interface IUserDialogService
{
    string? ChooseRepositoryFolder(string? currentPath);

    string? ChooseHtmlExportPath(string suggestedFileName);

    string? ChooseTextExportPath(string suggestedFileName);

    void ShowError(string title, string message);
}

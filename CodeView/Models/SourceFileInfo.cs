using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeView.Models;

public sealed class SourceFileInfo : INotifyPropertyChanged
{
    private bool _isIncluded = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public required string FullPath { get; init; }

    public required string RelativePath { get; init; }

    public required string Extension { get; init; }

    public required IReadOnlyList<string> Lines { get; init; }

    public int LineCount => Lines.Count;

    public bool IsIncluded
    {
        get => _isIncluded;
        set
        {
            if (_isIncluded == value)
            {
                return;
            }

            _isIncluded = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

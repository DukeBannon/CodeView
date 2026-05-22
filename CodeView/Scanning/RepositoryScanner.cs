using CodeView.Models;
using System.Diagnostics;
using System.IO;

namespace CodeView.Scanning;

public sealed class RepositoryScanner
{
    private static readonly HashSet<string> IncludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".xaml",
        ".csproj",
        ".sln",
        ".json",
        ".sql",
        ".md",
        ".txt",
        ".config",
        ".xml",
        ".yml",
        ".yaml",
        ".cbl",
        ".cob",
        ".cpy",
        ".pas",
        ".dpr",
        ".dfm",
        ".asm",
        ".s",
        ".inc",
        ".mac"
    };

    private static readonly HashSet<string> ExcludedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        "bin",
        "obj",
        "packages",
        "node_modules",
        ".idea",
        ".vscode"
    };

    private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".user",
        ".suo",
        ".cache",
        ".pdb",
        ".dll",
        ".exe"
    };

    private static readonly string[] TodoKeywords = ["TODO", "FIXME", "HACK", "BUG", "REVIEW"];

    public Task<ScanResult> ScanAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Scan(repositoryPath, cancellationToken), cancellationToken);
    }

    private static ScanResult Scan(string repositoryPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            throw new InvalidOperationException("Choose a repository folder before scanning.");
        }

        if (!Directory.Exists(repositoryPath))
        {
            throw new DirectoryNotFoundException($"Repository folder was not found: {repositoryPath}");
        }

        var root = Path.GetFullPath(repositoryPath);
        var files = EnumerateFiles(root, cancellationToken)
            .Select(path => ReadSourceFile(root, path))
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var todoItems = files
            .SelectMany(FindTodoItems)
            .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.LineNumber)
            .ToList();

        return new ScanResult
        {
            RepositoryPath = root,
            GeneratedAt = DateTimeOffset.Now,
            GitInfo = ReadGitInfo(root),
            Files = files,
            TodoItems = todoItems
        };
    }

    private static IEnumerable<string> EnumerateFiles(string directory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<string> childDirectories;
        IEnumerable<string> childFiles;

        try
        {
            childDirectories = Directory.EnumerateDirectories(directory);
            childFiles = Directory.EnumerateFiles(directory);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }

        foreach (var file in childFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldIncludeFile(file))
            {
                yield return file;
            }
        }

        foreach (var childDirectory in childDirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileName(childDirectory);
            if (ExcludedFolders.Contains(name))
            {
                continue;
            }

            foreach (var file in EnumerateFiles(childDirectory, cancellationToken))
            {
                yield return file;
            }
        }
    }

    private static bool ShouldIncludeFile(string path)
    {
        var extension = Path.GetExtension(path);
        if (ExcludedExtensions.Contains(extension))
        {
            return false;
        }

        return IncludedExtensions.Contains(extension);
    }

    private static SourceFileInfo ReadSourceFile(string root, string path)
    {
        var lines = File.ReadAllLines(path);
        var relativePath = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

        return new SourceFileInfo
        {
            FullPath = path,
            RelativePath = relativePath,
            Extension = Path.GetExtension(path),
            Lines = lines
        };
    }

    private static IEnumerable<TodoItem> FindTodoItems(SourceFileInfo file)
    {
        for (var index = 0; index < file.Lines.Count; index++)
        {
            var line = file.Lines[index];
            var keyword = TodoKeywords.FirstOrDefault(value =>
                line.Contains(value, StringComparison.OrdinalIgnoreCase));

            if (keyword is null)
            {
                continue;
            }

            yield return new TodoItem
            {
                RelativePath = file.RelativePath,
                LineNumber = index + 1,
                Keyword = keyword,
                Text = line.Trim()
            };
        }
    }

    private static GitRepositoryInfo ReadGitInfo(string repositoryPath)
    {
        var root = RunGit(repositoryPath, "rev-parse --show-toplevel");
        if (string.IsNullOrWhiteSpace(root))
        {
            return new GitRepositoryInfo();
        }

        var branch = RunGit(repositoryPath, "branch --show-current");
        if (string.IsNullOrWhiteSpace(branch))
        {
            branch = RunGit(repositoryPath, "rev-parse --short HEAD");
        }

        var commit = RunGit(repositoryPath, "rev-parse --short HEAD");
        var status = RunGit(repositoryPath, "status --porcelain");

        return new GitRepositoryInfo
        {
            RootPath = root,
            BranchName = branch,
            CommitHash = commit,
            IsDirty = status is not null ? status.Length > 0 : null
        };
    }

    private static string? RunGit(string workingDirectory, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(1500);

            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }
}

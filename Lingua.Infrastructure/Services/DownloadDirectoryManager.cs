using Microsoft.Extensions.Logging;

namespace Lingua.Infrastructure.Services;

public class DownloadDirectoryManager : IDownloadDirectoryManager
{
    private readonly ILogger<DownloadDirectoryManager> _logger;
    private string? _cachedDirectory;

    public DownloadDirectoryManager(ILogger<DownloadDirectoryManager> logger)
    {
        _logger = logger;
    }

    public string GetDownloadDirectory()
    {
        if (!string.IsNullOrEmpty(_cachedDirectory))
            return _cachedDirectory;

        _cachedDirectory = FindDownloadDirectory();
        return _cachedDirectory;
    }

    public void EnsureDirectoryExists()
    {
        var directory = GetDownloadDirectory();
        Directory.CreateDirectory(directory);
        _logger.LogInformation("Ensured download directory exists: {Directory}", directory);
    }

    private string FindDownloadDirectory()
    {
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

        if (!string.IsNullOrEmpty(assemblyDirectory))
        {
            var possiblePaths = new[]
            {
                // From bin/Debug/net9.0/ -> ../../../../downloads (solution root)
                Path.Combine(assemblyDirectory, "..", "..", "..", "..", "downloads"),
                // From bin/Debug/net9.0/ -> ../../../../../downloads (alternative)
                Path.Combine(assemblyDirectory, "..", "..", "..", "..", "..", "downloads"),
                // From current working directory
                Path.Combine(Directory.GetCurrentDirectory(), "downloads"),
                // From AppContext BaseDirectory
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "downloads")
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    var normalizedPath = Path.GetFullPath(path);
                    // Check if solution root exists (look for .sln file in parent)
                    var solutionRoot = Path.GetDirectoryName(normalizedPath);
                    if (!string.IsNullOrEmpty(solutionRoot) &&
                        Directory.GetFiles(solutionRoot, "*.sln").Length > 0)
                    {
                        _logger.LogInformation("Found download directory: {Directory}", normalizedPath);
                        return normalizedPath;
                    }
                }
                catch
                {
                    // Ignore path resolution errors
                }
            }
        }

        // Fallback to current directory
        var fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
        _logger.LogWarning("Could not find solution root, using fallback directory: {Directory}", fallbackPath);
        return fallbackPath;
    }
}


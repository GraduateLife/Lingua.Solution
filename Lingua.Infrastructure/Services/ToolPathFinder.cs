using Microsoft.Extensions.Logging;

namespace Lingua.Infrastructure.Services;

public class ToolPathFinder : IToolPathFinder
{
    private readonly ILogger<ToolPathFinder> _logger;

    public ToolPathFinder(ILogger<ToolPathFinder> logger)
    {
        _logger = logger;
    }

    public string? FindToolPath(string toolName, string? configuredPath = null)
    {
        // 1. 优先使用配置中指定的路径
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            _logger.LogInformation("Found {Tool} at configured path: {Path}", toolName, configuredPath);
            return configuredPath;
        }

        // 2. 查找 Infrastructure 项目的 Tools 目录
        var infrastructureToolsPath = GetInfrastructureToolsPath(toolName);
        if (!string.IsNullOrEmpty(infrastructureToolsPath))
        {
            return infrastructureToolsPath;
        }


        // 4. 在 PATH 环境变量中查找
        var pathEnvResult = FindInPath(GetExecutableName(toolName));
        if (!string.IsNullOrEmpty(pathEnvResult))
        {
            return pathEnvResult;
        }

        // 5. 查找常见安装位置（平台特定）
        var commonPath = FindInCommonLocations(toolName);
        if (!string.IsNullOrEmpty(commonPath))
        {
            return commonPath;
        }

        _logger.LogWarning("Could not find {Tool} in any location", toolName);
        return null;
    }

    public bool ValidateToolPath(string? toolPath)
    {
        if (string.IsNullOrWhiteSpace(toolPath))
            return false;

        return File.Exists(toolPath);
    }

    private string? GetInfrastructureToolsPath(string toolName)
    {
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

        if (!string.IsNullOrEmpty(assemblyDirectory))
        {
            var possiblePaths = new[]
            {
                // From Infrastructure bin/Debug/net9.0/ -> Tools
                Path.Combine(assemblyDirectory, "Tools"),
                // From Api bin/Debug/net9.0/ -> ../../Lingua.Infrastructure/bin/Debug/net9.0/Tools
                Path.Combine(assemblyDirectory, "..", "..", "Lingua.Infrastructure", "bin", "Debug", "net9.0", "Tools"),
                Path.Combine(assemblyDirectory, "..", "..", "Lingua.Infrastructure", "bin", "Release", "net9.0", "Tools"),
                // From solution root
                Path.Combine(Directory.GetCurrentDirectory(), "Lingua.Infrastructure", "bin", "Debug", "net9.0", "Tools"),
                Path.Combine(Directory.GetCurrentDirectory(), "Lingua.Infrastructure", "bin", "Release", "net9.0", "Tools"),
            };

            var exeName = GetExecutableName(toolName);
            foreach (var path in possiblePaths)
            {
                try
                {
                    var normalizedPath = Path.GetFullPath(path);
                    var fullPath = Path.Combine(normalizedPath, exeName);
                    if (File.Exists(fullPath))
                    {
                        _logger.LogInformation("Found {Tool} in Infrastructure Tools directory: {Path}", toolName, fullPath);
                        return fullPath;
                    }
                }
                catch
                {
                    // Ignore path resolution errors
                }
            }
        }

        return null;
    }

    private string GetExecutableName(string toolName)
    {
        return OperatingSystem.IsWindows() ? $"{toolName}.exe" : toolName;
    }

    private string? FindInPath(string executableName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return null;

        var paths = pathEnv.Split(Path.PathSeparator);
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, executableName);
            if (File.Exists(fullPath))
            {
                _logger.LogInformation("Found {Executable} in PATH: {Path}", executableName, fullPath);
                return fullPath;
            }
        }

        return null;
    }

    private string? FindInCommonLocations(string toolName)
    {
        if (!OperatingSystem.IsWindows())
            return null;

        var exeName = GetExecutableName(toolName);
        var commonPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", toolName, exeName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), toolName, exeName),
            Path.Combine(@"C:\", toolName, exeName)
        };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path))
            {
                _logger.LogInformation("Found {Tool} in common location: {Path}", toolName, path);
                return path;
            }
        }

        return null;
    }
}


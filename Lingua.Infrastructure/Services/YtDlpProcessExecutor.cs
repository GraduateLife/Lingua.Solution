using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Lingua.Infrastructure.Services;

public class YtDlpProcessExecutor : IYtDlpProcessExecutor
{
    private readonly ILogger<YtDlpProcessExecutor> _logger;
    private readonly IToolPathFinder _toolPathFinder;

    public YtDlpProcessExecutor(
        ILogger<YtDlpProcessExecutor> logger,
        IToolPathFinder toolPathFinder)
    {
        _logger = logger;
        _toolPathFinder = toolPathFinder;
    }

    public async Task<YtDlpExecutionResult> ExecuteAsync(
        string videoUrl,
        string outputTemplate,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        var ytDlpPath = _toolPathFinder.FindToolPath("yt-dlp");
        if (string.IsNullOrEmpty(ytDlpPath))
        {
            throw new InvalidOperationException("yt-dlp 未找到。请确保已安装 yt-dlp 并添加到 PATH 环境变量中。");
        }

        // Build yt-dlp command arguments
        var arguments = $"-o \"{outputTemplate}\" --no-playlist --format \"best[ext=mp4]/best\" \"{videoUrl}\"";

        _logger.LogDebug("Executing: {YtDlpPath} {Arguments}", ytDlpPath, arguments);
        _logger.LogInformation("Downloading video from: {VideoUrl} using yt-dlp", videoUrl);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };

        using var process = new Process { StartInfo = processStartInfo };

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                _logger.LogInformation("yt-dlp output: {Output}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);
                // yt-dlp writes progress to stderr, so log as info
                _logger.LogInformation("yt-dlp stderr: {Error}", e.Data);
            }
        };

        process.Start();

        // Start asynchronous reading of output streams
        // This is required for the OutputDataReceived and ErrorDataReceived events to fire
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait for completion with cancellation support
        await process.WaitForExitAsync(cancellationToken);

        // CRITICAL: Ensure process has fully exited and all output is read
        // This prevents the process from hanging due to full output buffers
        process.WaitForExit(); // Ensure process is completely terminated

        // Give event handlers a moment to finish processing any remaining data
        // This ensures all buffered output is captured before we read the StringBuilder
        await Task.Delay(100, cancellationToken);

        // Get all output (should be fully captured by now)
        var allOutput = outputBuilder.ToString();
        var allError = errorBuilder.ToString();

        _logger.LogInformation("yt-dlp exit code: {ExitCode}", process.ExitCode);
        _logger.LogInformation("yt-dlp stdout: {Output}", allOutput);
        if (!string.IsNullOrEmpty(allError))
        {
            _logger.LogInformation("yt-dlp stderr: {Error}", allError);
        }

        return new YtDlpExecutionResult
        {
            ExitCode = process.ExitCode,
            Output = allOutput,
            Error = allError
        };
    }
}


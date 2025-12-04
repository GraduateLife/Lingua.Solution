using Lingua.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lingua.Infrastructure.Services;

/// <summary>
/// 视频下载服务实现（使用 yt-dlp）
/// </summary>
public class VideoDownloadService : IVideoDownloadService
{
    private readonly ILogger<VideoDownloadService> _logger;
    private readonly IDownloadDirectoryManager _directoryManager;
    private readonly IYtDlpProcessExecutor _processExecutor;
    private readonly IVideoFileNameGenerator _fileNameGenerator;

    public VideoDownloadService(
        ILogger<VideoDownloadService> logger,
        IDownloadDirectoryManager directoryManager,
        IYtDlpProcessExecutor processExecutor,
        IVideoFileNameGenerator fileNameGenerator)
    {
        _logger = logger;
        _directoryManager = directoryManager;
        _processExecutor = processExecutor;
        _fileNameGenerator = fileNameGenerator;

        // Ensure download directory exists
        _directoryManager.EnsureDirectoryExists();
    }

    public async Task<Stream> DownloadVideoAsync(string videoUrl, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            throw new ArgumentException("Video URL cannot be empty", nameof(videoUrl));
        }

        if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid video URL", nameof(videoUrl));
        }

        string? tempFilePath = null;
        try
        {
            _logger.LogInformation("Starting video download for: {VideoUrl}", videoUrl);

            // Generate filename
            var fileName = _fileNameGenerator.GenerateFileName(videoUrl);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var downloadDirectory = _directoryManager.GetDownloadDirectory();
            var outputTemplate = Path.Combine(downloadDirectory, $"{fileNameWithoutExt}.%(ext)s");

            // Execute yt-dlp
            var result = await _processExecutor.ExecuteAsync(
                videoUrl,
                outputTemplate,
                downloadDirectory,
                cancellationToken);

            if (!result.Success)
            {
                var errorMessage = !string.IsNullOrEmpty(result.Error) ? result.Error : result.Output;
                _logger.LogError("yt-dlp failed with exit code {ExitCode}: {Error}", result.ExitCode, errorMessage);
                throw new Exception($"yt-dlp 下载失败 (退出代码: {result.ExitCode}): {errorMessage}");
            }

            // Wait a bit for file system to sync
            await Task.Delay(500, cancellationToken);

            // Find the downloaded file
            var actualFilePath = FindDownloadedFile(fileNameWithoutExt, downloadDirectory);
            if (string.IsNullOrEmpty(actualFilePath) || !File.Exists(actualFilePath))
            {
                // Log all files in temp directory for debugging
                var allFiles = Directory.GetFiles(downloadDirectory);
                _logger.LogWarning("Could not find downloaded file. Download directory files: {Files}",
                    string.Join(", ", allFiles.Select(Path.GetFileName)));
                throw new Exception($"无法找到下载的视频文件。下载目录: {downloadDirectory}");
            }

            tempFilePath = actualFilePath;
            var fileInfo = new FileInfo(actualFilePath);
            _logger.LogInformation("Video downloaded successfully to: {FilePath}, Size: {Size} bytes",
                actualFilePath, fileInfo.Length);

            // Return file stream directly - files in downloads folder are kept, not deleted
            // Use PersistentFileStream to ensure file is not deleted after streaming
            return new PersistentFileStream(actualFilePath, _logger);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Video download cancelled for: {VideoUrl}", videoUrl);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading video from {VideoUrl}", videoUrl);

            // Don't delete files in downloads folder even on error - keep them for debugging
            // Only log the error, but preserve the file
            if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
            {
                _logger.LogWarning("Error occurred but keeping downloaded file for inspection: {FilePath}", tempFilePath);
            }

            throw;
        }
    }

    public string GenerateVideoFileName(string videoUrl)
    {
        return _fileNameGenerator.GenerateFileName(videoUrl);
    }

    private string? FindDownloadedFile(string fileNamePrefix, string downloadDirectory)
    {
        // Look for files that start with the filename prefix
        var files = Directory.GetFiles(downloadDirectory, $"{fileNamePrefix}.*");
        if (files.Length > 0)
        {
            _logger.LogDebug("Found {Count} file(s) matching pattern {Pattern}: {Files}",
                files.Length, $"{fileNamePrefix}.*", string.Join(", ", files.Select(Path.GetFileName)));
            return files.FirstOrDefault();
        }

        // Also try to find any recently created video files (fallback)
        _logger.LogWarning("No files found matching pattern {Pattern}, checking all files in download directory", $"{fileNamePrefix}.*");
        var allFiles = Directory.GetFiles(downloadDirectory)
            .OrderByDescending(f => new FileInfo(f).CreationTime)
            .Take(5)
            .ToArray();

        if (allFiles.Length > 0)
        {
            _logger.LogDebug("Recent files in download directory: {Files}",
                string.Join(", ", allFiles.Select(Path.GetFileName)));
        }

        return null;
    }
}


namespace Lingua.Api.Models;

/// <summary>
/// Response model for video download operations.
/// </summary>
public class VideoDownloadResponse
{
    /// <summary>
    /// Gets the video URL.
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// Gets the downloaded file name.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// Gets the formatted file size (e.g., "1.5 MB").
    /// </summary>
    public string FileSizeFormatted { get; init; } = string.Empty;

    /// <summary>
    /// Gets the download duration in seconds.
    /// </summary>
    public double Duration { get; init; }
}


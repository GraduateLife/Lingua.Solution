namespace Lingua.Api.Models;

/// <summary>
/// Response model for video metadata queries.
/// </summary>
public class VideoMetadataResponse
{
    /// <summary>
    /// Gets a value indicating whether the video file exists.
    /// </summary>
    public bool Exists { get; init; }

    /// <summary>
    /// Gets the video URL.
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets the full file path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long? FileSize { get; init; }

    /// <summary>
    /// Gets the formatted file size (e.g., "1.5 MB").
    /// </summary>
    public string? FileSizeFormatted { get; init; }

    /// <summary>
    /// Gets the file creation time in UTC.
    /// </summary>
    public DateTime? CreatedTime { get; init; }

    /// <summary>
    /// Gets the file last modified time in UTC.
    /// </summary>
    public DateTime? ModifiedTime { get; init; }
}


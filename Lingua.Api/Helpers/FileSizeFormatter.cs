namespace Lingua.Api.Helpers;

/// <summary>
/// Helper class for formatting file sizes.
/// </summary>
public static class FileSizeFormatter
{
    /// <summary>
    /// Formats a file size in bytes to a human-readable string.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <returns>A formatted string representing the file size (e.g., "1.5 MB").</returns>
    public static string Format(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}


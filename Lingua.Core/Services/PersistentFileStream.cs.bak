using Microsoft.Extensions.Logging;

namespace Lingua.Core.Services;

/// <summary>
/// A file stream wrapper that keeps the file after disposal (does not delete it)
/// </summary>
public class PersistentFileStream : FileStream
{
    private readonly string _filePath;
    private readonly ILogger? _logger;

    public PersistentFileStream(string filePath, ILogger? logger = null)
        : base(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 8192, useAsync: true)
    {
        _filePath = filePath;
        _logger = logger;
        _logger?.LogDebug("Opened persistent file stream: {FilePath}", filePath);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        if (disposing)
        {
            // Do NOT delete the file - keep it in downloads folder
            _logger?.LogDebug("Closed persistent file stream (file kept): {FilePath}", _filePath);
            
            // Verify file still exists
            if (File.Exists(_filePath))
            {
                var fileInfo = new FileInfo(_filePath);
                _logger?.LogInformation("File preserved in downloads folder: {FilePath}, Size: {Size} bytes", 
                    _filePath, fileInfo.Length);
            }
            else
            {
                _logger?.LogWarning("File was deleted after stream closed: {FilePath}", _filePath);
            }
        }
    }
}


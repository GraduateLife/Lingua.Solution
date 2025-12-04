namespace Lingua.Core.Services;

public interface IVideoDownloadService
{
    Task<Stream> DownloadVideoAsync(string videoUrl, CancellationToken cancellationToken = default);
    string GenerateVideoFileName(string videoUrl);
}


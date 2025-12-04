using System.Security.Cryptography;
using System.Text;

namespace Lingua.Core.Services;

public class VideoFileNameGenerator : IVideoFileNameGenerator
{
    public string GenerateFileName(string videoUrl)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            return "video.mp4";
        }

        var urlMd5 = GetUrlHash(videoUrl);
        return $"video_{urlMd5}.mp4";
    }

    public string GetUrlHash(string url)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(url));
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        // Return first 8 characters of MD5 hash
        return hashString.Substring(0, Math.Min(8, hashString.Length));
    }
}


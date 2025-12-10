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
        
        // 从URL中提取文件名（去掉扩展名）
        var fileName = ExtractFileNameFromUrl(videoUrl);
        
        return $"{fileName}_{urlMd5}.mp4";
    }

    private string ExtractFileNameFromUrl(string url)
    {
        var lastSlashIndex = url.LastIndexOf('/');
        if (lastSlashIndex < 0 || lastSlashIndex >= url.Length - 1)
        {
            return "video";
        }

        var fileName = url.Substring(lastSlashIndex + 1);
        var lastDotIndex = fileName.LastIndexOf('.');
        var fileNameWithoutExtension = lastDotIndex > 0 
            ? fileName.Substring(0, lastDotIndex) 
            : fileName;
        
        return string.IsNullOrWhiteSpace(fileNameWithoutExtension) ? "video" : fileNameWithoutExtension;
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


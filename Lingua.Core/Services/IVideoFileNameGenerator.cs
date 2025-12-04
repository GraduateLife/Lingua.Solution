namespace Lingua.Core.Services;

/// <summary>
/// 视频文件名生成器接口
/// </summary>
public interface IVideoFileNameGenerator
{
    /// <summary>
    /// 生成视频文件名
    /// </summary>
    string GenerateFileName(string videoUrl);
    
    /// <summary>
    /// 获取 URL 的哈希值
    /// </summary>
    string GetUrlHash(string url);
}


namespace Lingua.Infrastructure.Services;

/// <summary>
/// 工具路径查找服务接口
/// 用于查找外部工具（yt-dlp, ffmpeg 等）的可执行文件路径
/// </summary>
public interface IToolPathFinder
{
    /// <summary>
    /// 查找工具的可执行文件路径
    /// </summary>
    /// <param name="toolName">工具名称（如 "yt-dlp", "ffmpeg"）</param>
    /// <param name="configuredPath">配置中指定的路径（可选）</param>
    /// <returns>找到的可执行文件完整路径，如果未找到则返回 null</returns>
    string? FindToolPath(string toolName, string? configuredPath = null);
    
    /// <summary>
    /// 验证工具路径是否有效
    /// </summary>
    bool ValidateToolPath(string? toolPath);
}


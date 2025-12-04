namespace Lingua.Infrastructure.Services;

/// <summary>
/// 下载目录管理服务接口
/// </summary>
public interface IDownloadDirectoryManager
{
    /// <summary>
    /// 获取下载目录路径
    /// </summary>
    string GetDownloadDirectory();
    
    /// <summary>
    /// 确保下载目录存在
    /// </summary>
    void EnsureDirectoryExists();
}


namespace Lingua.Infrastructure.Services;

/// <summary>
/// yt-dlp 进程执行器接口
/// </summary>
public interface IYtDlpProcessExecutor
{
    /// <summary>
    /// 执行 yt-dlp 下载命令
    /// </summary>
    /// <param name="videoUrl">视频 URL</param>
    /// <param name="outputTemplate">输出文件模板</param>
    /// <param name="workingDirectory">工作目录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    Task<YtDlpExecutionResult> ExecuteAsync(
        string videoUrl, 
        string outputTemplate,
        string workingDirectory,
        CancellationToken cancellationToken = default);
}


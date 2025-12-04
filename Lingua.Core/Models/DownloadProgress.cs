namespace Lingua.Core.Models;

/// <summary>
/// 下载进度信息
/// </summary>
public class DownloadProgress
{
    /// <summary>
    /// 下载百分比 (0-100)
    /// </summary>
    public double? Percentage { get; set; }
    
    /// <summary>
    /// 已下载字节数
    /// </summary>
    public long? DownloadedBytes { get; set; }
    
    /// <summary>
    /// 总字节数
    /// </summary>
    public long? TotalBytes { get; set; }
    
    /// <summary>
    /// 下载速度（字节/秒）
    /// </summary>
    public double? SpeedBytesPerSecond { get; set; }
    
    /// <summary>
    /// 预计剩余时间
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    
    /// <summary>
    /// 状态消息（原始输出行）
    /// </summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}


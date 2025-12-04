namespace Lingua.Infrastructure.Services;

/// <summary>
/// yt-dlp 执行结果
/// </summary>
public class YtDlpExecutionResult
{
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool Success => ExitCode == 0;
}


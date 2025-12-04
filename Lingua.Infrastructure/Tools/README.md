# Infrastructure Tools 目录

此目录用于存放 Infrastructure 项目所需的工具和可执行文件。

## yt-dlp

本项目使用 [yt-dlp](https://github.com/yt-dlp/yt-dlp) 来下载视频。

### 自动下载

运行以下 PowerShell 脚本自动下载最新版本的 yt-dlp：

```powershell
powershell -ExecutionPolicy Bypass -File Lingua.Infrastructure\Tools\download-yt-dlp.ps1
```

### 手动下载

1. 访问 [yt-dlp Releases](https://github.com/yt-dlp/yt-dlp/releases/latest)
2. 下载 `yt-dlp.exe` (Windows) 或 `yt-dlp` (Linux/Mac)
3. 将文件放置在此 `Tools` 目录中

### 验证安装

下载完成后，项目会自动检测并使用 Infrastructure 项目 `Tools` 目录中的 yt-dlp。

如果 yt-dlp 未找到，项目会按以下顺序查找：

1. Infrastructure 项目 `Tools` 目录（优先）
2. 系统 PATH 环境变量
3. 常见的安装位置

### 注意事项

- Windows 用户需要 `yt-dlp.exe`
- Linux/Mac 用户需要 `yt-dlp`（无扩展名）
- 确保文件具有执行权限（Linux/Mac）


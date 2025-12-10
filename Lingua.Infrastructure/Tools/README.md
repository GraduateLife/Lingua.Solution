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

## ffmpeg

本项目使用 [ffmpeg](https://ffmpeg.org/) 来修复和处理视频文件，解决 yt-dlp 下载时可能出现的格式问题。

### 自动下载

运行以下 PowerShell 脚本自动下载最新版本的 ffmpeg：

```powershell
powershell -ExecutionPolicy Bypass -File Lingua.Infrastructure\Tools\download-ffmpeg.ps1
```

**注意**：自动下载需要安装 7-Zip。如果未安装 7-Zip，脚本会提供手动安装指导。

### 手动下载

1. 访问 [ffmpeg Windows Builds](https://www.gyan.dev/ffmpeg/builds/)
2. 下载 `ffmpeg-release-full.7z`
3. 解压文件（需要 7-Zip）
4. 从解压后的 `bin` 文件夹中复制以下文件到 `Tools` 目录：
   - `ffmpeg.exe`
   - `ffprobe.exe`（可选，但推荐）
   - `ffplay.exe`（可选）

### 使用包管理器安装

**Windows (winget):**

```powershell
winget install ffmpeg
```

然后从安装目录复制 `ffmpeg.exe` 到 `Tools` 目录。

**Windows (choco):**

```powershell
choco install ffmpeg
```

### 验证安装

下载完成后，项目会自动检测并使用 Infrastructure 项目 `Tools` 目录中的 ffmpeg。

如果 ffmpeg 未找到，项目会按以下顺序查找：

1. Infrastructure 项目 `Tools` 目录（优先）
2. 系统 PATH 环境变量
3. 常见的安装位置

### 注意事项

- Windows 用户需要 `ffmpeg.exe`
- Linux/Mac 用户需要 `ffmpeg`（无扩展名）
- 确保文件具有执行权限（Linux/Mac）
- ffmpeg 用于自动修复 yt-dlp 下载的视频格式问题

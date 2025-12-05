using Lingua.Core.Models;
using Lingua.Core.Services;
using Lingua.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure request timeout for long-running operations (video downloads)
// Add request timeout service (will be disabled for video download endpoints)
builder.Services.AddRequestTimeouts();

// Configure Kestrel server options for long-running requests
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Infrastructure services (all yt-dlp related services)
builder.Services.AddSingleton<IToolPathFinder, ToolPathFinder>();
builder.Services.AddSingleton<IDownloadDirectoryManager, DownloadDirectoryManager>();
builder.Services.AddScoped<IYtDlpProcessExecutor, YtDlpProcessExecutor>();

// Add Core services (business logic only)
builder.Services.AddSingleton<IVideoFileNameGenerator, VideoFileNameGenerator>();

// Add video download service (implementation in Infrastructure, interface in Core)
builder.Services.AddScoped<IVideoDownloadService, Lingua.Infrastructure.Services.VideoDownloadService>();

// Add CORS - allow all origins in development for easier testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });

});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable request timeouts middleware (individual endpoints can disable it)
app.UseRequestTimeouts();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS - try default policy first, fallback to AllowAll in development
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}

// Simple test endpoint - echo back the received string
app.MapGet("/api/test/echo", (ILogger<Program> logger) =>
    {
        return Results.Ok(new { received = "Hello, World!" });
    });

// Video metadata endpoint (GET) - Returns metadata of already downloaded videos
// Only checks for existing files, does not download
app.MapGet("/api/download/metadata", async (
    string? url,
    IVideoDownloadService service,
    IDownloadDirectoryManager directoryManager,
    IVideoFileNameGenerator fileNameGenerator,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Results.BadRequest(new { error = "URL parameter is required" });
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Results.BadRequest(new { error = "Invalid URL format" });
        }

        logger.LogInformation("Received GET video metadata request for: {VideoUrl}", url);

        try
        {
            // Generate filename to check
            var fileName = fileNameGenerator.GenerateFileName(url);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var downloadDirectory = directoryManager.GetDownloadDirectory();

            // Find the downloaded file
            var files = Directory.GetFiles(downloadDirectory, $"{fileNameWithoutExt}.*");
            var existingFile = files.FirstOrDefault();

            if (string.IsNullOrEmpty(existingFile) || !File.Exists(existingFile))
            {
                logger.LogInformation("Video file not found for: {VideoUrl}", url);
                return Results.Ok(new
                {
                    success = false,
                    exists = false,
                    url = url,
                    fileName = fileName,
                    message = "视频文件不存在，请先下载"
                });
            }

            // Get file metadata
            var fileInfo = new FileInfo(existingFile);
            logger.LogInformation("Found video file: {FileName}, Size: {Size} bytes",
                existingFile, fileInfo.Length);

            return Results.Ok(new
            {
                success = true,
                exists = true,
                url = url,
                fileName = Path.GetFileName(existingFile),
                filePath = existingFile,
                fileSize = fileInfo.Length,
                fileSizeFormatted = FormatFileSize(fileInfo.Length),
                createdTime = fileInfo.CreationTimeUtc,
                modifiedTime = fileInfo.LastWriteTimeUtc,
                message = "视频文件已存在"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking video metadata: {Message}", ex.Message);
            return Results.Ok(new
            {
                success = false,
                error = ex.Message,
                errorType = ex.GetType().Name,
                url = url
            });
        }
    })
    .WithName("GetVideoMetadata")
    .WithOpenApi();

// Helper method to format file size
static string FormatFileSize(long bytes)
{
    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
    double len = bytes;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1)
    {
        order++;
        len = len / 1024;
    }
    return $"{len:0.##} {sizes[order]}";
}

// Video download endpoint (GET) - Downloads video and returns JSON metadata
app.MapGet("/api/download", async (
    string? url,
    IVideoDownloadService service,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Results.BadRequest(new { error = "URL parameter is required" });
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Results.BadRequest(new { error = "Invalid URL format" });
        }

        logger.LogInformation("Received GET video download request for: {VideoUrl}", url);

        Stream? videoStream = null;
        try
        {
            logger.LogInformation("Starting video download process...");
            var startTime = DateTime.UtcNow;

            // Download video using yt-dlp
            var downloadTask = service.DownloadVideoAsync(url, cancellationToken);
            logger.LogInformation("Waiting for video download to complete...");

            videoStream = await downloadTask;
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            var fileName = service.GenerateVideoFileName(url);
            logger.LogInformation("Got filename: {FileName}", fileName);

            if (videoStream == null)
            {
                logger.LogError("Video stream is null");
                return Results.Ok(new
                {
                    success = false,
                    error = "视频流为空",
                    url = url,
                    duration = duration.TotalSeconds
                });
            }

            var fileLength = videoStream.CanSeek ? videoStream.Length : -1;
            logger.LogInformation("Video downloaded, file: {FileName}, Size: {Size} bytes",
                fileName, fileLength);

            // Return JSON metadata instead of streaming the video
            return Results.Ok(new
            {
                success = true,
                url = url,
                fileName = fileName,
                fileSize = fileLength,
                fileSizeFormatted = fileLength > 0 ? FormatFileSize(fileLength) : "unknown",
                duration = duration.TotalSeconds,
                message = "视频下载成功，可使用 /api/video/download/test?url={url} 查看详细信息"
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument in video download request");
            videoStream?.Dispose();
            return Results.BadRequest(new
            {
                success = false,
                error = ex.Message,
                url = url
            });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Video download was cancelled");
            videoStream?.Dispose();
            return Results.Ok(new
            {
                success = false,
                error = "下载被取消",
                url = url
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in video download: {Message}", ex.Message);
            videoStream?.Dispose();
            return Results.Ok(new
            {
                success = false,
                error = ex.Message,
                errorType = ex.GetType().Name,
                url = url
            });
        }
        finally
        {
            // Ensure stream is disposed
            videoStream?.Dispose();
        }
    })
    .WithName("DownloadVideoGet")
    .WithOpenApi()
    .DisableRequestTimeout();

// Video download endpoint (POST) - Proxy mode: backend downloads and streams the file
app.MapPost("/api/video/download", async (
    Lingua.Core.Models.VideoDownloadRequest request,
    Lingua.Core.Services.IVideoDownloadService service,
    ILogger<Program> logger,
    HttpContext context,
    CancellationToken cancellationToken) =>
    {
        logger.LogInformation("Received video download request (proxy mode) for: {VideoUrl}", request.VideoUrl);

        Stream? videoStream = null;
        try
        {
            // Download video using yt-dlp (backend acts as proxy)
            videoStream = await service.DownloadVideoAsync(request.VideoUrl, cancellationToken);
            var fileName = service.GenerateVideoFileName(request.VideoUrl);

            if (videoStream == null)
            {
                logger.LogError("Video stream is null");
                return Results.Problem("视频流为空", statusCode: 500);
            }

            var fileLength = videoStream.CanSeek ? videoStream.Length : -1;
            logger.LogInformation("Video downloaded, streaming file: {FileName}, Size: {Size} bytes",
                fileName, fileLength);

            // Set response headers
            context.Response.ContentType = "video/mp4";
            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\"";
            if (fileLength > 0)
            {
                context.Response.ContentLength = fileLength;
            }

            // Stream the file directly to response
            if (videoStream.CanSeek)
            {
                videoStream.Position = 0;
            }

            logger.LogInformation("Starting to stream video file to client");
            await videoStream.CopyToAsync(context.Response.Body, cancellationToken);
            logger.LogInformation("Video file streamed successfully");

            return Results.Empty;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument in video download request");
            videoStream?.Dispose();
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Video download was cancelled");
            videoStream?.Dispose();
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in video download proxy: {Message}", ex.Message);
            videoStream?.Dispose();

            // Only return error if response hasn't started
            if (!context.Response.HasStarted)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "下载视频时发生错误"
                );
            }

            // If response has started, we can't change the status code
            return Results.Empty;
        }
        finally
        {
            // Ensure stream is disposed
            videoStream?.Dispose();
        }
    })
    .WithName("DownloadVideo")
    .WithOpenApi()
    .DisableAntiforgery()
    .RequireCors(app.Environment.IsDevelopment() ? "AllowAll" : "default")
    .DisableRequestTimeout();

app.Run();
using Lingua.Api.Helpers;
using Lingua.Api.Models;
using Lingua.Core.Models;
using Lingua.Core.Services;
using Lingua.Infrastructure.Services;

namespace Lingua.Api.Endpoints;

/// <summary>
/// Video download and metadata endpoints.
/// </summary>
public static class VideoEndpoints
{
    /// <summary>
    /// Maps video-related endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapVideoEndpoints(this IEndpointRouteBuilder app)
    {
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
            // Validate URL using helper
            var validation = UrlValidator.ValidateUrl(url);
            if (!validation.IsValid)
            {
                return Results.BadRequest(ApiResponse<VideoMetadataResponse>.Failure(validation.ErrorMessage!));
            }

            var validatedUrl = validation.Value!;
            logger.LogInformation("Received GET video metadata request for: {VideoUrl}", validatedUrl);

            try
            {
                // Generate filename to check
                var fileName = fileNameGenerator.GenerateFileName(validatedUrl);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var downloadDirectory = directoryManager.GetDownloadDirectory();

                // Find the downloaded file
                var files = Directory.GetFiles(downloadDirectory, $"{fileNameWithoutExt}.*");
                var existingFile = files.FirstOrDefault();

                if (string.IsNullOrEmpty(existingFile) || !File.Exists(existingFile))
                {
                    logger.LogInformation("Video file not found for: {VideoUrl}", validatedUrl);
                    return Results.Ok(ApiResponse<VideoMetadataResponse>.Ok(new VideoMetadataResponse
                    {
                        Exists = false,
                        Url = validatedUrl,
                        FileName = fileName
                    }, "视频文件不存在，请先下载"));
                }

                // Get file metadata
                var fileInfo = new FileInfo(existingFile);
                logger.LogInformation("Found video file: {FileName}, Size: {Size} bytes",
                    existingFile, fileInfo.Length);

                return Results.Ok(ApiResponse<VideoMetadataResponse>.Ok(new VideoMetadataResponse
                {
                    Exists = true,
                    Url = validatedUrl,
                    FileName = Path.GetFileName(existingFile),
                    FilePath = existingFile,
                    FileSize = fileInfo.Length,
                    FileSizeFormatted = FileSizeFormatter.Format(fileInfo.Length),
                    CreatedTime = fileInfo.CreationTimeUtc,
                    ModifiedTime = fileInfo.LastWriteTimeUtc
                }, "视频文件已存在"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking video metadata: {Message}", ex.Message);
                return Results.Ok(ApiResponse<VideoMetadataResponse>.Failure(ex.Message, ex.GetType().Name));
            }
        })
        .WithName("GetVideoMetadata")
        .WithOpenApi();

        // Video download endpoint (GET) - Downloads video and returns JSON metadata
        app.MapGet("/api/download", async (
            string? url,
            IVideoDownloadService service,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            // Validate URL using helper
            var validation = UrlValidator.ValidateUrl(url);
            if (!validation.IsValid)
            {
                return Results.BadRequest(ApiResponse<VideoDownloadResponse>.Failure(validation.ErrorMessage!));
            }

            var validatedUrl = validation.Value!;
            logger.LogInformation("Received GET video download request for: {VideoUrl}", validatedUrl);

            Stream? videoStream = null;
            try
            {
                logger.LogInformation("Starting video download process...");
                var startTime = DateTime.UtcNow;

                // Download video using yt-dlp
                var downloadTask = service.DownloadVideoAsync(validatedUrl, cancellationToken);
                logger.LogInformation("Waiting for video download to complete...");

                videoStream = await downloadTask;
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                var fileName = service.GenerateVideoFileName(validatedUrl);
                logger.LogInformation("Got filename: {FileName}", fileName);

                if (videoStream == null)
                {
                    logger.LogError("Video stream is null");
                    return Results.Ok(ApiResponse<VideoDownloadResponse>.Failure("视频流为空"));
                }

                var fileLength = videoStream.CanSeek ? videoStream.Length : -1;
                logger.LogInformation("Video downloaded, file: {FileName}, Size: {Size} bytes",
                    fileName, fileLength);

                // Return JSON metadata instead of streaming the video
                return Results.Ok(ApiResponse<VideoDownloadResponse>.Ok(new VideoDownloadResponse
                {
                    Url = validatedUrl,
                    FileName = fileName,
                    FileSize = fileLength > 0 ? fileLength : 0,
                    FileSizeFormatted = fileLength > 0 ? FileSizeFormatter.Format(fileLength) : "unknown",
                    Duration = duration.TotalSeconds
                }, "视频下载成功，可使用 /api/download/metadata?url={url} 查看详细信息"));
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid argument in video download request");
                videoStream?.Dispose();
                return Results.BadRequest(ApiResponse<VideoDownloadResponse>.Failure(ex.Message));
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Video download was cancelled");
                videoStream?.Dispose();
                return Results.Ok(ApiResponse<VideoDownloadResponse>.Failure("下载被取消"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in video download: {Message}", ex.Message);
                videoStream?.Dispose();
                return Results.Ok(ApiResponse<VideoDownloadResponse>.Failure(ex.Message, ex.GetType().Name));
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
        app.MapPost("/api/download", async (
            VideoDownloadRequest request,
            IVideoDownloadService service,
            ILogger<Program> logger,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // Validate URL using helper
            var validation = UrlValidator.ValidateUrl(request.VideoUrl);
            if (!validation.IsValid)
            {
                return Results.BadRequest(new { error = validation.ErrorMessage });
            }

            var validatedUrl = validation.Value!;
            logger.LogInformation("Received video download request (proxy mode) for: {VideoUrl}", validatedUrl);

            Stream? videoStream = null;
            try
            {
                // Download video using yt-dlp (backend acts as proxy)
                videoStream = await service.DownloadVideoAsync(validatedUrl, cancellationToken);
                var fileName = service.GenerateVideoFileName(validatedUrl);

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
        .WithName("DownloadVideoPost")
        .WithOpenApi()
        .DisableAntiforgery()
        .RequireCors("DevCors")
        .DisableRequestTimeout();

        return app;
    }
}


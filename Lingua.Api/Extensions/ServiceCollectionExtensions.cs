using Lingua.Core.Services;
using Lingua.Infrastructure.Services;

namespace Lingua.Api.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Lingua services to the service collection.
    /// </summary>
    public static IServiceCollection AddLinguaServices(this IServiceCollection services)
    {
        // Configure request timeout for long-running operations (video downloads)
        // Will be disabled for video download endpoints
        services.AddRequestTimeouts();

        // Add OpenAPI
        services.AddOpenApi();

        // Add Infrastructure services (all yt-dlp related services)
        services.AddSingleton<IToolPathFinder, ToolPathFinder>();
        services.AddSingleton<IDownloadDirectoryManager, DownloadDirectoryManager>();
        services.AddScoped<IYtDlpProcessExecutor, YtDlpProcessExecutor>();

        // Add Core services (business logic only)
        services.AddSingleton<IVideoFileNameGenerator, VideoFileNameGenerator>();

        // Add video download service (implementation in Infrastructure, interface in Core)
        services.AddScoped<IVideoDownloadService, Lingua.Infrastructure.Services.VideoDownloadService>();

        // Add CORS - allow all origins in development for easier testing
        services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });

        return services;
    }

    /// <summary>
    /// Configures Kestrel server options for long-running requests.
    /// </summary>
    public static IWebHostBuilder AddLinguaKestrel(this IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.ConfigureKestrel(options =>
        {
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
        });

        return webHostBuilder;
    }
}


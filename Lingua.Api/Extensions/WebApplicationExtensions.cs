namespace Lingua.Api.Extensions;

/// <summary>
/// Extension methods for configuring the HTTP request pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline for Lingua API.
    /// </summary>
    public static WebApplication UseLinguaMiddleware(this WebApplication app)
    {
        // Enable request timeouts middleware (individual endpoints can disable it)
        app.UseRequestTimeouts();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseCors("DevCors");
        }

        return app;
    }
}


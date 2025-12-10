namespace Lingua.Api.Endpoints;

/// <summary>
/// Test endpoints for API health checks and basic functionality.
/// </summary>
public static class TestEndpoints
{
    /// <summary>
    /// Maps test endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        // Simple test endpoint - echo back the received string
        app.MapGet("/api/test/ping", (ILogger<Program> logger) =>
        {
            return Results.Ok(new { pong = DateTime.UtcNow });
        })
        .WithName("PingTest")
        .WithOpenApi();

        return app;
    }
}


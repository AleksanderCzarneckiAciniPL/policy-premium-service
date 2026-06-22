namespace PolicyPremium.Api.Endpoints;

/// <summary>
/// Diagnostics endpoints (liveness).
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow }))
            .WithName("Health")
            .WithTags("Diagnostics")
            .WithSummary("Liveness check")
            .WithDescription("Returns 200 with a status payload while the service is running. Used by orchestrators and uptime checks.")
            .Produces(StatusCodes.Status200OK);

        return app;
    }
}

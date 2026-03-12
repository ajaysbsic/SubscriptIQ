namespace SubscriptIQ.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow 
        }))
        .WithName("Health")
        .WithTags("Health")
        .WithOpenApi();

        app.MapGet("/", () => Results.Ok(new 
        { 
            name = "SubscriptIQ API",
            version = "1.0.0",
            description = "Plug-and-play subscription, billing, and entitlement platform",
            endpoints = new[]
            {
                "/health",
                "/api/tenants",
                "/api/plans",
                "/api/subscriptions",
                "/api/entitlements",
                "/api/webhooks/stripe"
            }
        }))
        .WithName("Root")
        .WithOpenApi();
    }
}

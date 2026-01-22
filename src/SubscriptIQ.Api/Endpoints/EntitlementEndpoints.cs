using Microsoft.AspNetCore.Mvc;
using SubscriptIQ.Core.Services;

namespace SubscriptIQ.Api.Endpoints;

public static class EntitlementEndpoints
{
    public static void MapEntitlementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/entitlements")
            .WithTags("Entitlements")
            .WithOpenApi();

        group.MapGet("/check-access", async (
            [FromQuery] Guid tenantId,
            [FromQuery] string featureKey,
            [FromServices] EntitlementService entitlementService) =>
        {
            var result = await entitlementService.CheckFeatureAccessAsync(tenantId, featureKey);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            return Results.Ok(new 
            { 
                hasAccess = result.Value,
                tenantId,
                featureKey
            });
        })
        .WithName("CheckFeatureAccess");

        group.MapGet("/check-usage", async (
            [FromQuery] Guid tenantId,
            [FromQuery] string featureKey,
            [FromQuery] int amount,
            [FromServices] EntitlementService entitlementService) =>
        {
            var result = await entitlementService.CheckUsageLimitAsync(tenantId, featureKey, amount);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            return Results.Ok(new 
            { 
                withinLimit = result.Value,
                tenantId,
                featureKey,
                requestedAmount = amount
            });
        })
        .WithName("CheckUsageLimit");

        group.MapPost("/increment-usage", async (
            [FromBody] IncrementUsageRequest request,
            [FromServices] EntitlementService entitlementService) =>
        {
            // First check if within limit
            var checkResult = await entitlementService.CheckUsageLimitAsync(
                request.TenantId, 
                request.FeatureKey, 
                request.Amount);

            if (checkResult.IsFailure)
                return Results.BadRequest(new { error = checkResult.Error });

            if (!checkResult.Value)
                return Results.BadRequest(new { error = "Usage limit exceeded" });

            // Increment usage
            var result = await entitlementService.IncrementUsageAsync(
                request.TenantId,
                request.FeatureKey,
                request.Amount);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            // Get updated usage
            var usageResult = await entitlementService.GetCurrentUsageAsync(
                request.TenantId, 
                request.FeatureKey);

            var limitResult = await entitlementService.GetUsageLimitAsync(
                request.TenantId, 
                request.FeatureKey);

            return Results.Ok(new 
            { 
                message = "Usage incremented successfully",
                currentUsage = usageResult.Value,
                limit = limitResult.Value,
                tenantId = request.TenantId,
                featureKey = request.FeatureKey
            });
        })
        .WithName("IncrementUsage");

        group.MapGet("/usage", async (
            [FromQuery] Guid tenantId,
            [FromQuery] string featureKey,
            [FromServices] EntitlementService entitlementService) =>
        {
            var usageResult = await entitlementService.GetCurrentUsageAsync(tenantId, featureKey);
            var limitResult = await entitlementService.GetUsageLimitAsync(tenantId, featureKey);

            return Results.Ok(new 
            { 
                currentUsage = usageResult.Value,
                limit = limitResult.Value,
                tenantId,
                featureKey
            });
        })
        .WithName("GetUsage");
    }
}

public record IncrementUsageRequest(Guid TenantId, string FeatureKey, int Amount);

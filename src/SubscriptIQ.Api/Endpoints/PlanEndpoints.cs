using Microsoft.AspNetCore.Mvc;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Api.Endpoints;

public static class PlanEndpoints
{
    public static void MapPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plans")
            .WithTags("Plans")
            .WithOpenApi();

        group.MapPost("/", async (
            [FromBody] CreatePlanRequest request,
            [FromServices] IPlanRepository planRepository,
            [FromServices] IUnitOfWork unitOfWork) =>
        {
            var price = Money.Create(request.PriceAmount, request.PriceCurrency);
            var plan = Plan.Create(
                request.Name,
                request.Description,
                price,
                request.BillingInterval,
                request.TrialDays);

            if (request.Features != null)
            {
                foreach (var feature in request.Features)
                {
                    plan.AddFeature(feature);
                }
            }

            await planRepository.AddAsync(plan);
            await unitOfWork.SaveChangesAsync();

            return Results.Created($"/api/plans/{plan.Id}", MapPlanToResponse(plan));
        })
        .WithName("CreatePlan");

        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] IPlanRepository planRepository) =>
        {
            var plan = await planRepository.GetByIdAsync(id);
            if (plan == null)
                return Results.NotFound();

            return Results.Ok(MapPlanToResponse(plan));
        })
        .WithName("GetPlan");

        group.MapGet("/", async (
            [FromQuery] bool? activeOnly,
            [FromServices] IPlanRepository planRepository) =>
        {
            var plans = activeOnly == true
                ? await planRepository.GetActiveAsync()
                : await planRepository.GetAllAsync();

            return Results.Ok(plans.Select(MapPlanToResponse));
        })
        .WithName("GetAllPlans");

        group.MapPost("/{id:guid}/entitlements", async (
            Guid id,
            [FromBody] CreateEntitlementRequest request,
            [FromServices] IPlanRepository planRepository,
            [FromServices] IEntitlementRepository entitlementRepository,
            [FromServices] IUnitOfWork unitOfWork) =>
        {
            var plan = await planRepository.GetByIdAsync(id);
            if (plan == null)
                return Results.NotFound("Plan not found");

            Entitlement entitlement = request.Type == "FeatureFlag"
                ? Entitlement.CreateFeatureFlag(id, request.FeatureKey, request.IsEnabled)
                : Entitlement.CreateUsageLimit(id, request.FeatureKey, request.Limit ?? 0);

            await entitlementRepository.AddAsync(entitlement);
            await unitOfWork.SaveChangesAsync();

            return Results.Created($"/api/plans/{id}/entitlements/{entitlement.Id}", new
            {
                id = entitlement.Id,
                planId = entitlement.PlanId,
                featureKey = entitlement.FeatureKey,
                type = entitlement.Type.ToString(),
                isEnabled = entitlement.IsEnabled,
                limit = entitlement.Limit
            });
        })
        .WithName("CreateEntitlement");
    }

    private static object MapPlanToResponse(Plan plan) => new
    {
        id = plan.Id,
        name = plan.Name,
        description = plan.Description,
        priceAmount = plan.Price.Amount,
        priceCurrency = plan.Price.Currency,
        billingInterval = plan.BillingInterval.ToString(),
        isActive = plan.IsActive,
        trialDays = plan.TrialDays,
        stripePriceId = plan.StripePriceId,
        features = plan.Features,
        createdAt = plan.CreatedAt,
        updatedAt = plan.UpdatedAt
    };
}

public record CreatePlanRequest(
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    BillingInterval BillingInterval,
    int? TrialDays,
    string[]? Features);

public record CreateEntitlementRequest(
    string FeatureKey,
    string Type,
    bool IsEnabled,
    int? Limit);

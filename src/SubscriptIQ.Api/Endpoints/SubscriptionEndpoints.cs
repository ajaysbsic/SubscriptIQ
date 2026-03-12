using Microsoft.AspNetCore.Mvc;
using SubscriptIQ.Core.Services;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Api.Endpoints;

public static class SubscriptionEndpoints
{
    public static void MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscriptions")
            .WithTags("Subscriptions")
            .WithOpenApi();

        group.MapPost("/", async (
            [FromBody] CreateSubscriptionRequest request,
            [FromServices] SubscriptionService subscriptionService) =>
        {
            var result = await subscriptionService.CreateSubscriptionAsync(
                request.TenantId,
                request.PlanId);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            var subscription = result.Value!;
            return Results.Created($"/api/subscriptions/{subscription.Id}", new
            {
                id = subscription.Id,
                tenantId = subscription.TenantId,
                planId = subscription.PlanId,
                status = subscription.Status.ToString(),
                startDate = subscription.StartDate,
                trialEndDate = subscription.TrialEndDate,
                currentPeriodStart = subscription.CurrentPeriodStart,
                currentPeriodEnd = subscription.CurrentPeriodEnd,
                createdAt = subscription.CreatedAt
            });
        })
        .WithName("CreateSubscription");

        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] SubscriptionService subscriptionService) =>
        {
            var subscription = await subscriptionService.GetSubscriptionAsync(id);
            if (subscription == null)
                return Results.NotFound();

            return Results.Ok(new
            {
                id = subscription.Id,
                tenantId = subscription.TenantId,
                planId = subscription.PlanId,
                status = subscription.Status.ToString(),
                startDate = subscription.StartDate,
                endDate = subscription.EndDate,
                trialEndDate = subscription.TrialEndDate,
                cancelledAt = subscription.CancelledAt,
                currentPeriodStart = subscription.CurrentPeriodStart,
                currentPeriodEnd = subscription.CurrentPeriodEnd,
                stripeSubscriptionId = subscription.StripeSubscriptionId,
                pendingDowngradePlanId = subscription.PendingDowngradePlanId,
                isActive = subscription.IsActive(),
                isInTrial = subscription.IsInTrial(),
                createdAt = subscription.CreatedAt,
                updatedAt = subscription.UpdatedAt
            });
        })
        .WithName("GetSubscription");

        group.MapGet("/tenant/{tenantId:guid}", async (
            Guid tenantId,
            [FromServices] SubscriptionService subscriptionService) =>
        {
            var subscriptions = await subscriptionService.GetTenantSubscriptionsAsync(tenantId);
            return Results.Ok(subscriptions.Select(s => new
            {
                id = s.Id,
                tenantId = s.TenantId,
                planId = s.PlanId,
                status = s.Status.ToString(),
                startDate = s.StartDate,
                currentPeriodEnd = s.CurrentPeriodEnd,
                isActive = s.IsActive()
            }));
        })
        .WithName("GetTenantSubscriptions");

        group.MapPost("/{id:guid}/cancel", async (
            Guid id,
            [FromBody] CancelSubscriptionRequest request,
            [FromServices] SubscriptionService subscriptionService) =>
        {
            var result = await subscriptionService.CancelSubscriptionAsync(
                id,
                request.Reason,
                request.Immediately);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            return Results.Ok(new { message = "Subscription cancelled successfully" });
        })
        .WithName("CancelSubscription");

        group.MapPost("/{id:guid}/upgrade", async (
            Guid id,
            [FromBody] ChangePlanRequest request,
            [FromServices] SubscriptionService subscriptionService) =>
        {
            var result = await subscriptionService.UpgradeSubscriptionAsync(id, request.NewPlanId);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            return Results.Ok(new { message = "Subscription upgraded successfully" });
        })
        .WithName("UpgradeSubscription");

        group.MapPost("/{id:guid}/downgrade", async (
            Guid id,
            [FromBody] ChangePlanRequest request,
            [FromServices] SubscriptionService subscriptionService) =>
        {
            var result = await subscriptionService.DowngradeSubscriptionAsync(id, request.NewPlanId);

            if (result.IsFailure)
                return Results.BadRequest(new { error = result.Error });

            return Results.Ok(new { message = "Subscription downgrade scheduled for end of billing period" });
        })
        .WithName("DowngradeSubscription");
    }
}

public record CreateSubscriptionRequest(Guid TenantId, Guid PlanId);
public record CancelSubscriptionRequest(string Reason, bool Immediately);
public record ChangePlanRequest(Guid NewPlanId);

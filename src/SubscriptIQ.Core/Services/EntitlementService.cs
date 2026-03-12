using SubscriptIQ.Core.Common;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;

namespace SubscriptIQ.Core.Services;

/// <summary>
/// Service for checking entitlements and feature access
/// </summary>
public class EntitlementService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IEntitlementRepository _entitlementRepository;
    private readonly IUsageTrackingRepository _usageTrackingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EntitlementService(
        ISubscriptionRepository subscriptionRepository,
        IEntitlementRepository entitlementRepository,
        IUsageTrackingRepository usageTrackingRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _entitlementRepository = entitlementRepository;
        _usageTrackingRepository = usageTrackingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> CheckFeatureAccessAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        var activeSubscription = subscriptions.FirstOrDefault(s => s.IsActive());

        if (activeSubscription == null)
            return Result.Success(false);

        var entitlement = await _entitlementRepository.GetByPlanAndFeatureAsync(activeSubscription.PlanId, featureKey, cancellationToken);
        
        if (entitlement == null)
            return Result.Success(false);

        return Result.Success(entitlement.IsEnabled);
    }

    public async Task<Result<bool>> CheckUsageLimitAsync(Guid tenantId, string featureKey, int requestedAmount = 1, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        var activeSubscription = subscriptions.FirstOrDefault(s => s.IsActive());

        if (activeSubscription == null)
            return Result.Success(false);

        var entitlement = await _entitlementRepository.GetByPlanAndFeatureAsync(activeSubscription.PlanId, featureKey, cancellationToken);
        
        if (entitlement == null || entitlement.Type != EntitlementType.UsageLimit)
            return Result.Success(true); // No limit set, allow access

        if (!entitlement.Limit.HasValue)
            return Result.Success(true); // Unlimited

        var usage = await _usageTrackingRepository.GetCurrentUsageAsync(activeSubscription.Id, featureKey, cancellationToken);
        
        if (usage == null)
        {
            // Create new usage tracking for current period
            usage = UsageTracking.Create(
                tenantId,
                activeSubscription.Id,
                featureKey,
                activeSubscription.CurrentPeriodStart,
                activeSubscription.CurrentPeriodEnd);
            
            await _usageTrackingRepository.AddAsync(usage, cancellationToken);
        }

        var wouldExceed = (usage.CurrentUsage + requestedAmount) > entitlement.Limit.Value;
        return Result.Success(!wouldExceed);
    }

    public async Task<Result> IncrementUsageAsync(Guid tenantId, string featureKey, int amount = 1, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        var activeSubscription = subscriptions.FirstOrDefault(s => s.IsActive());

        if (activeSubscription == null)
            return Result.Failure("No active subscription found");

        var usage = await _usageTrackingRepository.GetCurrentUsageAsync(activeSubscription.Id, featureKey, cancellationToken);
        
        if (usage == null)
        {
            usage = UsageTracking.Create(
                tenantId,
                activeSubscription.Id,
                featureKey,
                activeSubscription.CurrentPeriodStart,
                activeSubscription.CurrentPeriodEnd);
            
            await _usageTrackingRepository.AddAsync(usage, cancellationToken);
        }

        var result = usage.IncrementUsage(amount);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _usageTrackingRepository.UpdateAsync(usage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<int>> GetCurrentUsageAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        var activeSubscription = subscriptions.FirstOrDefault(s => s.IsActive());

        if (activeSubscription == null)
            return Result.Success(0);

        var usage = await _usageTrackingRepository.GetCurrentUsageAsync(activeSubscription.Id, featureKey, cancellationToken);
        
        return Result.Success(usage?.CurrentUsage ?? 0);
    }

    public async Task<Result<int?>> GetUsageLimitAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        var activeSubscription = subscriptions.FirstOrDefault(s => s.IsActive());

        if (activeSubscription == null)
            return Result.Success<int?>(null);

        var entitlement = await _entitlementRepository.GetByPlanAndFeatureAsync(activeSubscription.PlanId, featureKey, cancellationToken);
        
        if (entitlement == null || entitlement.Type != EntitlementType.UsageLimit)
            return Result.Success<int?>(null);

        return Result.Success(entitlement.Limit);
    }
}

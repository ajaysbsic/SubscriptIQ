using SubscriptIQ.Core.Common;

namespace SubscriptIQ.Core.Entities;

/// <summary>
/// Tracks usage for features with usage limits
/// </summary>
public class UsageTracking : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public string FeatureKey { get; private set; }
    public int CurrentUsage { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }

    private UsageTracking() { } // EF Core

    private UsageTracking(Guid tenantId, Guid subscriptionId, string featureKey, DateTime periodStart, DateTime periodEnd) : base()
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        FeatureKey = featureKey;
        CurrentUsage = 0;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }

    public static UsageTracking Create(Guid tenantId, Guid subscriptionId, string featureKey, DateTime periodStart, DateTime periodEnd)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            throw new ArgumentException("Feature key is required", nameof(featureKey));

        if (periodEnd <= periodStart)
            throw new ArgumentException("Period end must be after period start");

        return new UsageTracking(tenantId, subscriptionId, featureKey, periodStart, periodEnd);
    }

    public Result<int> IncrementUsage(int amount = 1)
    {
        if (amount < 0)
            return Result.Failure<int>("Amount cannot be negative");

        if (DateTime.UtcNow > PeriodEnd)
            return Result.Failure<int>("Usage period has ended");

        CurrentUsage += amount;
        MarkAsUpdated();
        
        return Result.Success(CurrentUsage);
    }

    public bool IsWithinLimit(int limit)
    {
        return CurrentUsage < limit;
    }

    public void Reset()
    {
        CurrentUsage = 0;
        MarkAsUpdated();
    }
}

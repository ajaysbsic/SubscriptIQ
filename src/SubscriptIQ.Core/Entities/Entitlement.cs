using SubscriptIQ.Core.Common;

namespace SubscriptIQ.Core.Entities;

/// <summary>
/// Represents an entitlement (feature flag or usage limit) for a plan
/// </summary>
public class Entitlement : Entity
{
    public Guid PlanId { get; private set; }
    public string FeatureKey { get; private set; }
    public EntitlementType Type { get; private set; }
    public bool IsEnabled { get; private set; }
    public int? Limit { get; private set; }
    public string? Metadata { get; private set; }

    private Entitlement() { } // EF Core

    private Entitlement(Guid planId, string featureKey, EntitlementType type, bool isEnabled, int? limit = null, string? metadata = null) : base()
    {
        PlanId = planId;
        FeatureKey = featureKey;
        Type = type;
        IsEnabled = isEnabled;
        Limit = limit;
        Metadata = metadata;
    }

    public static Entitlement CreateFeatureFlag(Guid planId, string featureKey, bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            throw new ArgumentException("Feature key is required", nameof(featureKey));

        return new Entitlement(planId, featureKey, EntitlementType.FeatureFlag, isEnabled);
    }

    public static Entitlement CreateUsageLimit(Guid planId, string featureKey, int limit)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            throw new ArgumentException("Feature key is required", nameof(featureKey));

        if (limit < 0)
            throw new ArgumentException("Limit cannot be negative", nameof(limit));

        return new Entitlement(planId, featureKey, EntitlementType.UsageLimit, true, limit);
    }

    public void Enable()
    {
        IsEnabled = true;
        MarkAsUpdated();
    }

    public void Disable()
    {
        IsEnabled = false;
        MarkAsUpdated();
    }

    public void UpdateLimit(int newLimit)
    {
        if (Type != EntitlementType.UsageLimit)
            throw new InvalidOperationException("Can only update limit for usage limit entitlements");

        if (newLimit < 0)
            throw new ArgumentException("Limit cannot be negative", nameof(newLimit));

        Limit = newLimit;
        MarkAsUpdated();
    }
}

public enum EntitlementType
{
    FeatureFlag,
    UsageLimit
}

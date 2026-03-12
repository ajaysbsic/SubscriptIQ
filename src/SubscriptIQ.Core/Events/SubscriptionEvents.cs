using SubscriptIQ.Core.Common;

namespace SubscriptIQ.Core.Events;

public class SubscriptionCreatedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid PlanId { get; }
    public DateTime StartDate { get; }
    public DateTime? TrialEndDate { get; }

    public SubscriptionCreatedEvent(Guid subscriptionId, Guid tenantId, Guid planId, DateTime startDate, DateTime? trialEndDate)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        TenantId = tenantId;
        PlanId = planId;
        StartDate = startDate;
        TrialEndDate = trialEndDate;
    }
}

public class SubscriptionActivatedEvent : DomainEvent
{
    public DateTime ActivatedAt { get; }

    public SubscriptionActivatedEvent(Guid subscriptionId, DateTime activatedAt)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        ActivatedAt = activatedAt;
    }
}

public class SubscriptionCancelledEvent : DomainEvent
{
    public DateTime CancelledAt { get; }
    public string Reason { get; }
    public bool ImmediateCancellation { get; }

    public SubscriptionCancelledEvent(Guid subscriptionId, DateTime cancelledAt, string reason, bool immediateCancellation)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        CancelledAt = cancelledAt;
        Reason = reason;
        ImmediateCancellation = immediateCancellation;
    }
}

public class SubscriptionUpgradedEvent : DomainEvent
{
    public Guid OldPlanId { get; }
    public Guid NewPlanId { get; }
    public DateTime UpgradedAt { get; }

    public SubscriptionUpgradedEvent(Guid subscriptionId, Guid oldPlanId, Guid newPlanId, DateTime upgradedAt)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        OldPlanId = oldPlanId;
        NewPlanId = newPlanId;
        UpgradedAt = upgradedAt;
    }
}

public class SubscriptionDowngradedEvent : DomainEvent
{
    public Guid OldPlanId { get; }
    public Guid NewPlanId { get; }
    public DateTime DowngradedAt { get; }
    public DateTime EffectiveDate { get; }

    public SubscriptionDowngradedEvent(Guid subscriptionId, Guid oldPlanId, Guid newPlanId, DateTime downgradedAt, DateTime effectiveDate)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        OldPlanId = oldPlanId;
        NewPlanId = newPlanId;
        DowngradedAt = downgradedAt;
        EffectiveDate = effectiveDate;
    }
}

public class SubscriptionRenewedEvent : DomainEvent
{
    public DateTime RenewedAt { get; }
    public DateTime NextBillingDate { get; }

    public SubscriptionRenewedEvent(Guid subscriptionId, DateTime renewedAt, DateTime nextBillingDate)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        RenewedAt = renewedAt;
        NextBillingDate = nextBillingDate;
    }
}

public class SubscriptionExpiredEvent : DomainEvent
{
    public DateTime ExpiredAt { get; }

    public SubscriptionExpiredEvent(Guid subscriptionId, DateTime expiredAt)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        ExpiredAt = expiredAt;
    }
}

public class SubscriptionPastDueEvent : DomainEvent
{
    public DateTime PastDueAt { get; }

    public SubscriptionPastDueEvent(Guid subscriptionId, DateTime pastDueAt)
        : base(subscriptionId, nameof(Entities.Subscription))
    {
        PastDueAt = pastDueAt;
    }
}

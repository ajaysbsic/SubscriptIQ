using SubscriptIQ.Core.Common;
using SubscriptIQ.Core.Events;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Core.Entities;

/// <summary>
/// Event-sourced subscription aggregate root
/// </summary>
public class Subscription : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public Guid? PendingDowngradePlanId { get; private set; }

    private Subscription() : base() { } // EF Core

    private Subscription(Guid tenantId, Guid planId, DateTime startDate, DateTime? trialEndDate) : base()
    {
        var @event = new SubscriptionCreatedEvent(Id, tenantId, planId, startDate, trialEndDate);
        ApplyEvent(@event);
    }

    public static Subscription Create(Guid tenantId, Guid planId, int? trialDays = null)
    {
        var startDate = DateTime.UtcNow;
        var trialEndDate = trialDays.HasValue ? startDate.AddDays(trialDays.Value) : (DateTime?)null;
        
        return new Subscription(tenantId, planId, startDate, trialEndDate);
    }

    public Result Activate()
    {
        if (Status == SubscriptionStatus.Active)
            return Result.Failure("Subscription is already active");

        var @event = new SubscriptionActivatedEvent(Id, DateTime.UtcNow);
        ApplyEvent(@event);
        
        return Result.Success();
    }

    public Result Cancel(string reason, bool immediately = false)
    {
        if (Status == SubscriptionStatus.Cancelled || Status == SubscriptionStatus.Expired)
            return Result.Failure("Subscription is already cancelled or expired");

        var @event = new SubscriptionCancelledEvent(Id, DateTime.UtcNow, reason, immediately);
        ApplyEvent(@event);
        
        return Result.Success();
    }

    public Result Upgrade(Guid newPlanId)
    {
        if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.Trial)
            return Result.Failure("Subscription must be active or in trial to upgrade");

        if (PlanId == newPlanId)
            return Result.Failure("Cannot upgrade to the same plan");

        var @event = new SubscriptionUpgradedEvent(Id, PlanId, newPlanId, DateTime.UtcNow);
        ApplyEvent(@event);
        
        return Result.Success();
    }

    public Result Downgrade(Guid newPlanId, DateTime effectiveDate)
    {
        if (Status != SubscriptionStatus.Active)
            return Result.Failure("Subscription must be active to downgrade");

        if (PlanId == newPlanId)
            return Result.Failure("Cannot downgrade to the same plan");

        if (effectiveDate < DateTime.UtcNow)
            return Result.Failure("Effective date cannot be in the past");

        var @event = new SubscriptionDowngradedEvent(Id, PlanId, newPlanId, DateTime.UtcNow, effectiveDate);
        ApplyEvent(@event);
        
        return Result.Success();
    }

    public Result Renew(DateTime nextBillingDate)
    {
        if (Status != SubscriptionStatus.Active)
            return Result.Failure("Only active subscriptions can be renewed");

        var @event = new SubscriptionRenewedEvent(Id, DateTime.UtcNow, nextBillingDate);
        ApplyEvent(@event);
        
        return Result.Success();
    }

    public Result MarkAsPastDue()
    {
        if (Status == SubscriptionStatus.Cancelled || Status == SubscriptionStatus.Expired)
            return Result.Failure("Cannot mark cancelled or expired subscription as past due");

        var @event = new SubscriptionPastDueEvent(Id, DateTime.UtcNow);
        ApplyEvent(@event);
        
        return Result.Success();
    }

    public Result Expire()
    {
        if (Status == SubscriptionStatus.Expired)
            return Result.Failure("Subscription is already expired");

        var @event = new SubscriptionExpiredEvent(Id, DateTime.UtcNow);
        ApplyEvent(@event);
        
        return Result.Success();
    }

    public void SetStripeSubscriptionId(string stripeSubscriptionId)
    {
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            throw new ArgumentException("Stripe subscription ID is required", nameof(stripeSubscriptionId));
        
        StripeSubscriptionId = stripeSubscriptionId;
        MarkAsUpdated();
    }

    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case SubscriptionCreatedEvent e:
                TenantId = e.TenantId;
                PlanId = e.PlanId;
                StartDate = e.StartDate;
                TrialEndDate = e.TrialEndDate;
                Status = e.TrialEndDate.HasValue ? SubscriptionStatus.Trial : SubscriptionStatus.Active;
                CurrentPeriodStart = e.StartDate;
                CurrentPeriodEnd = e.TrialEndDate ?? e.StartDate.AddMonths(1);
                break;
            
            case SubscriptionActivatedEvent e:
                Status = SubscriptionStatus.Active;
                break;
            
            case SubscriptionCancelledEvent e:
                Status = SubscriptionStatus.Cancelled;
                CancelledAt = e.CancelledAt;
                if (e.ImmediateCancellation)
                    EndDate = e.CancelledAt;
                else
                    EndDate = CurrentPeriodEnd;
                break;
            
            case SubscriptionUpgradedEvent e:
                PlanId = e.NewPlanId;
                // Upgrade takes effect immediately
                break;
            
            case SubscriptionDowngradedEvent e:
                // Store pending downgrade to apply at period end
                PendingDowngradePlanId = e.NewPlanId;
                break;
            
            case SubscriptionRenewedEvent e:
                CurrentPeriodStart = CurrentPeriodEnd;
                CurrentPeriodEnd = e.NextBillingDate;
                // Apply pending downgrade if exists
                if (PendingDowngradePlanId.HasValue)
                {
                    PlanId = PendingDowngradePlanId.Value;
                    PendingDowngradePlanId = null;
                }
                break;
            
            case SubscriptionExpiredEvent e:
                Status = SubscriptionStatus.Expired;
                EndDate = e.ExpiredAt;
                break;
            
            case SubscriptionPastDueEvent e:
                Status = SubscriptionStatus.PastDue;
                break;
        }
        
        MarkAsUpdated();
    }

    public bool IsInTrial() => Status == SubscriptionStatus.Trial && 
                                TrialEndDate.HasValue && 
                                DateTime.UtcNow < TrialEndDate.Value;

    public bool IsActive() => Status == SubscriptionStatus.Active || IsInTrial();
}

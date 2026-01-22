using SubscriptIQ.Core.Common;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;

namespace SubscriptIQ.Core.Services;

/// <summary>
/// Service for managing subscriptions
/// </summary>
public class SubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Subscription>> CreateSubscriptionAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan == null)
            return Result.Failure<Subscription>("Plan not found");

        if (!plan.IsActive)
            return Result.Failure<Subscription>("Plan is not active");

        var subscription = Subscription.Create(tenantId, planId, plan.TrialDays);
        
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subscription);
    }

    public async Task<Result> CancelSubscriptionAsync(Guid subscriptionId, string reason, bool immediately = false, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
            return Result.Failure("Subscription not found");

        var result = subscription.Cancel(reason, immediately);
        if (result.IsFailure)
            return result;

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpgradeSubscriptionAsync(Guid subscriptionId, Guid newPlanId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
            return Result.Failure("Subscription not found");

        var newPlan = await _planRepository.GetByIdAsync(newPlanId, cancellationToken);
        if (newPlan == null)
            return Result.Failure("New plan not found");

        if (!newPlan.IsActive)
            return Result.Failure("New plan is not active");

        var result = subscription.Upgrade(newPlanId);
        if (result.IsFailure)
            return result;

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DowngradeSubscriptionAsync(Guid subscriptionId, Guid newPlanId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
            return Result.Failure("Subscription not found");

        var newPlan = await _planRepository.GetByIdAsync(newPlanId, cancellationToken);
        if (newPlan == null)
            return Result.Failure("New plan not found");

        if (!newPlan.IsActive)
            return Result.Failure("New plan is not active");

        // Downgrade takes effect at the end of current billing period
        var effectiveDate = subscription.CurrentPeriodEnd;
        var result = subscription.Downgrade(newPlanId, effectiveDate);
        if (result.IsFailure)
            return result;

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RenewSubscriptionAsync(Guid subscriptionId, DateTime nextBillingDate, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
            return Result.Failure("Subscription not found");

        var result = subscription.Renew(nextBillingDate);
        if (result.IsFailure)
            return result;

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Subscription?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetTenantSubscriptionsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
    }
}

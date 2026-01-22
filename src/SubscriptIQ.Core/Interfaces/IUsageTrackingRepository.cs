using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Core.Interfaces;

public interface IUsageTrackingRepository
{
    Task<UsageTracking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UsageTracking?> GetCurrentUsageAsync(Guid subscriptionId, string featureKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageTracking>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task AddAsync(UsageTracking usageTracking, CancellationToken cancellationToken = default);
    Task UpdateAsync(UsageTracking usageTracking, CancellationToken cancellationToken = default);
}

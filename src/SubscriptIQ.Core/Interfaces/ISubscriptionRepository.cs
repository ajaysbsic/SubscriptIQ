using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Core.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetExpiringSoonAsync(int days, CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}

using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Core.Interfaces;

public interface IEntitlementRepository
{
    Task<Entitlement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entitlement>> GetByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<Entitlement?> GetByPlanAndFeatureAsync(Guid planId, string featureKey, CancellationToken cancellationToken = default);
    Task AddAsync(Entitlement entitlement, CancellationToken cancellationToken = default);
    Task UpdateAsync(Entitlement entitlement, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Core.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}

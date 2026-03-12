using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Core.Interfaces;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Plan>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Plan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(Plan plan, CancellationToken cancellationToken = default);
}

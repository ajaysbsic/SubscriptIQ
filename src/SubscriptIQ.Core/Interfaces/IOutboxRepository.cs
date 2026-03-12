using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Core.Interfaces;

public interface IOutboxRepository
{
    Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default);
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}

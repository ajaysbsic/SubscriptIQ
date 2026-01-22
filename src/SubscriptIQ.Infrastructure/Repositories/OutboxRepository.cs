using Microsoft.EntityFrameworkCore;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Infrastructure.Data;

namespace SubscriptIQ.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly SubscriptIQDbContext _context;

    public OutboxRepository(SubscriptIQDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(o => !o.IsProcessed && o.RetryCount < 5)
            .OrderBy(o => o.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.OutboxMessages.Update(message);
        return Task.CompletedTask;
    }
}

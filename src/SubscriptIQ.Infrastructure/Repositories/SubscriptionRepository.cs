using Microsoft.EntityFrameworkCore;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Core.ValueObjects;
using SubscriptIQ.Infrastructure.Data;

namespace SubscriptIQ.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SubscriptIQDbContext _context;

    public SubscriptionRepository(SubscriptIQDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetExpiringSoonAsync(int days, CancellationToken cancellationToken = default)
    {
        var expirationDate = DateTime.UtcNow.AddDays(days);
        return await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active && 
                       s.CurrentPeriodEnd <= expirationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
    }

    public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}

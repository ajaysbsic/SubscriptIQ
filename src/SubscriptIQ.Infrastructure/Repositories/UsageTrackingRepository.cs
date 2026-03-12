using Microsoft.EntityFrameworkCore;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Infrastructure.Data;

namespace SubscriptIQ.Infrastructure.Repositories;

public class UsageTrackingRepository : IUsageTrackingRepository
{
    private readonly SubscriptIQDbContext _context;

    public UsageTrackingRepository(SubscriptIQDbContext context)
    {
        _context = context;
    }

    public async Task<UsageTracking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UsageTrackings
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<UsageTracking?> GetCurrentUsageAsync(Guid subscriptionId, string featureKey, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UsageTrackings
            .FirstOrDefaultAsync(u => u.SubscriptionId == subscriptionId && 
                                     u.FeatureKey == featureKey && 
                                     u.PeriodStart <= now && 
                                     u.PeriodEnd >= now, cancellationToken);
    }

    public async Task<IEnumerable<UsageTracking>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.UsageTrackings
            .Where(u => u.SubscriptionId == subscriptionId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UsageTracking usageTracking, CancellationToken cancellationToken = default)
    {
        await _context.UsageTrackings.AddAsync(usageTracking, cancellationToken);
    }

    public Task UpdateAsync(UsageTracking usageTracking, CancellationToken cancellationToken = default)
    {
        _context.UsageTrackings.Update(usageTracking);
        return Task.CompletedTask;
    }
}

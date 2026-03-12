using Microsoft.EntityFrameworkCore;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Infrastructure.Data;

namespace SubscriptIQ.Infrastructure.Repositories;

public class EntitlementRepository : IEntitlementRepository
{
    private readonly SubscriptIQDbContext _context;

    public EntitlementRepository(SubscriptIQDbContext context)
    {
        _context = context;
    }

    public async Task<Entitlement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Entitlements
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Entitlement>> GetByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        return await _context.Entitlements
            .Where(e => e.PlanId == planId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Entitlement?> GetByPlanAndFeatureAsync(Guid planId, string featureKey, CancellationToken cancellationToken = default)
    {
        return await _context.Entitlements
            .FirstOrDefaultAsync(e => e.PlanId == planId && e.FeatureKey == featureKey, cancellationToken);
    }

    public async Task AddAsync(Entitlement entitlement, CancellationToken cancellationToken = default)
    {
        await _context.Entitlements.AddAsync(entitlement, cancellationToken);
    }

    public Task UpdateAsync(Entitlement entitlement, CancellationToken cancellationToken = default)
    {
        _context.Entitlements.Update(entitlement);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entitlement = await GetByIdAsync(id, cancellationToken);
        if (entitlement != null)
        {
            _context.Entitlements.Remove(entitlement);
        }
    }
}

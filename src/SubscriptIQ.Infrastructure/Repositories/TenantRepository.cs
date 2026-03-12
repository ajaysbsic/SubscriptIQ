using Microsoft.EntityFrameworkCore;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Infrastructure.Data;

namespace SubscriptIQ.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly SubscriptIQDbContext _context;

    public TenantRepository(SubscriptIQDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.ApiKey == apiKey, cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        return Task.CompletedTask;
    }
}

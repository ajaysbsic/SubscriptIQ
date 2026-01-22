using Microsoft.EntityFrameworkCore;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Infrastructure.Data;

namespace SubscriptIQ.Infrastructure.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly SubscriptIQDbContext _context;

    public PlanRepository(SubscriptIQDbContext context)
    {
        _context = context;
    }

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Plan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        await _context.Plans.AddAsync(plan, cancellationToken);
    }

    public Task UpdateAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        _context.Plans.Update(plan);
        return Task.CompletedTask;
    }
}

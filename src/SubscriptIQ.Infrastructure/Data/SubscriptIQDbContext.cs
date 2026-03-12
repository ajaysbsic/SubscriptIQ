using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SubscriptIQ.Core.Common;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using System.Text.Json;

namespace SubscriptIQ.Infrastructure.Data;

public class SubscriptIQDbContext : DbContext, IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public SubscriptIQDbContext(DbContextOptions<SubscriptIQDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Plan> Plans { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<Entitlement> Entitlements { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<UsageTracking> UsageTrackings { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptIQDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Process domain events before saving
        await ProcessDomainEventsAsync(cancellationToken);
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            _currentTransaction?.Commit();
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        return Task.CompletedTask;
    }

    private async Task ProcessDomainEventsAsync(CancellationToken cancellationToken)
    {
        var aggregateRoots = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(x => x.DomainEvents)
            .ToList();

        aggregateRoots.ForEach(x => x.ClearDomainEvents());

        // Save events to outbox
        foreach (var domainEvent in domainEvents)
        {
            var eventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            var outboxMessage = OutboxMessage.Create(domainEvent.GetType().Name, eventData);
            OutboxMessages.Add(outboxMessage);
        }

        await Task.CompletedTask;
    }
}

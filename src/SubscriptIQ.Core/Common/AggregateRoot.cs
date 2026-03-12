namespace SubscriptIQ.Core.Common;

/// <summary>
/// Base class for aggregate roots that support event sourcing
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public int Version { get; protected set; }

    protected AggregateRoot() : base()
    {
        Version = 0;
    }

    protected AggregateRoot(Guid id) : base(id)
    {
        Version = 0;
    }

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        Version++;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void ApplyEvent(DomainEvent @event)
    {
        AddDomainEvent(@event);
        Apply(@event);
    }

    protected abstract void Apply(DomainEvent @event);
}

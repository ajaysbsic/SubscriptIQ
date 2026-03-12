namespace SubscriptIQ.Core.Common;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class DomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid AggregateId { get; protected set; }
    public string AggregateType { get; protected set; }

    protected DomainEvent(Guid aggregateId, string aggregateType)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        AggregateId = aggregateId;
        AggregateType = aggregateType;
    }
}

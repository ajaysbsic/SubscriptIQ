using SubscriptIQ.Core.Common;

namespace SubscriptIQ.Core.Entities;

/// <summary>
/// Outbox pattern for reliable event publishing
/// </summary>
public class OutboxMessage : Entity
{
    public string EventType { get; private set; }
    public string EventData { get; private set; }
    public bool IsProcessed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    private OutboxMessage() { } // EF Core

    private OutboxMessage(string eventType, string eventData) : base()
    {
        EventType = eventType;
        EventData = eventData;
        IsProcessed = false;
        RetryCount = 0;
    }

    public static OutboxMessage Create(string eventType, string eventData)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required", nameof(eventType));

        if (string.IsNullOrWhiteSpace(eventData))
            throw new ArgumentException("Event data is required", nameof(eventData));

        return new OutboxMessage(eventType, eventData);
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void IncrementRetryCount(string? errorMessage = null)
    {
        RetryCount++;
        ErrorMessage = errorMessage;
        MarkAsUpdated();
    }
}

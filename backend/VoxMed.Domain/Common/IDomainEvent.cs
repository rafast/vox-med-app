namespace VoxMed.Domain.Common;

/// <summary>
/// Marker interface for domain events in DDD
/// Domain events represent something important that happened in the domain
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event occurrence
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When this event occurred
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Version of the event schema (for event evolution)
    /// </summary>
    int Version { get; }
}

/// <summary>
/// Base implementation of domain event
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public virtual int Version { get; protected set; } = 1;

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}

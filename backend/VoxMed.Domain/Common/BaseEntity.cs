using System.ComponentModel.DataAnnotations;

namespace VoxMed.Domain.Common;

/// <summary>
/// Base entity class that provides common properties for all domain entities
/// Implements common DDD patterns like auditing and soft delete
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    [Key]
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTime DateAdded { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated
    /// </summary>
    public DateTime DateUpdated { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag - marks entity as deleted without physically removing it
    /// </summary>
    public bool IsDeleted { get; protected set; } = false;

    /// <summary>
    /// When the entity was soft deleted (null if not deleted)
    /// </summary>
    public DateTime? DateDeleted { get; protected set; }

    /// <summary>
    /// Who created this entity (user ID)
    /// </summary>
    public Guid? CreatedBy { get; protected set; }

    /// <summary>
    /// Who last updated this entity (user ID)
    /// </summary>
    public Guid? UpdatedBy { get; protected set; }

    /// <summary>
    /// Version number for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? Version { get; protected set; }

    /// <summary>
    /// Updates the entity's audit information
    /// </summary>
    /// <param name="updatedBy">ID of the user making the update</param>
    public virtual void UpdateAuditInfo(Guid? updatedBy = null)
    {
        DateUpdated = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Marks the entity as deleted (soft delete)
    /// </summary>
    /// <param name="deletedBy">ID of the user performing the delete</param>
    public virtual void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DateDeleted = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        DateUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores a soft-deleted entity
    /// </summary>
    /// <param name="restoredBy">ID of the user performing the restore</param>
    public virtual void Restore(Guid? restoredBy = null)
    {
        IsDeleted = false;
        DateDeleted = null;
        UpdatedBy = restoredBy;
        DateUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the creation audit information
    /// </summary>
    /// <param name="createdBy">ID of the user creating the entity</param>
    public virtual void SetCreationAuditInfo(Guid? createdBy = null)
    {
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}

/// <summary>
/// Base entity for aggregate roots in DDD
/// Provides additional functionality for domain events and business rule validation
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be published
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to remove</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Validates business rules for this aggregate
    /// Override in derived classes to implement specific business rules
    /// </summary>
    /// <returns>List of validation errors (empty if valid)</returns>
    public virtual List<string> ValidateBusinessRules()
    {
        return new List<string>();
    }
}

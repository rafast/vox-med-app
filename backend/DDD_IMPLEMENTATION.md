# Domain-Driven Design Implementation

## Overview

We have successfully applied key Domain-Driven Design (DDD) principles to the VoxMed scheduling domain. This refactoring improves maintainability, consistency, and provides a solid foundation for complex business logic.

## DDD Concepts Applied

### 1. Base Entity (`BaseEntity`)
A common base class providing standard properties and behavior for all domain entities:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime DateAdded { get; protected set; }
    public DateTime DateUpdated { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTime? DateDeleted { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
    public byte[]? Version { get; protected set; } // Optimistic concurrency
    
    // Audit methods
    public virtual void UpdateAuditInfo(Guid? updatedBy = null)
    public virtual void SoftDelete(Guid? deletedBy = null)
    public virtual void Restore(Guid? restoredBy = null)
    public virtual void SetCreationAuditInfo(Guid? createdBy = null)
}
```

**Benefits:**
- **Consistency**: All entities have standard audit fields
- **Soft Delete**: Implements soft delete pattern across all entities
- **Audit Trail**: Tracks who created/modified entities
- **Optimistic Concurrency**: Version field for handling concurrent updates

### 2. Aggregate Root (`AggregateRoot`)
Extends BaseEntity to provide domain event functionality:

```csharp
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    public void ClearDomainEvents()
    public virtual List<string> ValidateBusinessRules()
}
```

**Benefits:**
- **Domain Events**: Enables decoupled communication between aggregates
- **Business Rules**: Centralized validation logic
- **Encapsulation**: Protects domain events from external modification

### 3. Domain Events (`IDomainEvent`)
Events that represent important business occurrences:

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    int Version { get; }
}

// Examples of domain events
public class DoctorScheduleCreatedEvent : DomainEvent
public class AppointmentCreatedEvent : DomainEvent
public class AppointmentCancelledEvent : DomainEvent
```

**Benefits:**
- **Decoupling**: Loose coupling between business operations
- **Event Sourcing**: Foundation for event sourcing if needed
- **Integration**: Enables integration with external systems
- **Audit**: Complete audit trail of business events

### 4. Value Objects (`ValueObject`)
Immutable objects defined by their attributes rather than identity:

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    // Equality operators and methods
    public override bool Equals(object? obj)
    public override int GetHashCode()
    public static bool operator ==(ValueObject left, ValueObject right)
}
```

**Benefits:**
- **Immutability**: Prevents accidental modification
- **Value Equality**: Compared by value, not reference
- **Side-Effect Free**: No behavioral side effects

## Refactored Entities

### 1. DoctorSchedule (Aggregate Root)

**Before:**
```csharp
public class DoctorSchedule
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Public setters, no business logic
}
```

**After:**
```csharp
public class DoctorSchedule : AggregateRoot
{
    // Properties with private setters
    public Guid DoctorId { get; private set; }
    public TimeOnly StartTime { get; private set; }
    
    // Factory method
    public static DoctorSchedule Create(...)
    
    // Business methods
    public void UpdateSchedule(...)
    public void Activate(...)
    public void Deactivate(...)
    
    // Business rules validation
    public override List<string> ValidateBusinessRules()
    
    // Query methods
    public bool IsEffectiveOn(DateOnly date)
    public int GetTotalSlotsPerDay()
}
```

**Improvements:**
- **Encapsulation**: Private setters protect data integrity
- **Factory Methods**: Controlled object creation with validation
- **Business Methods**: Rich domain behavior
- **Domain Events**: Raises events for important changes
- **Validation**: Built-in business rule validation

### 2. Appointment (Aggregate Root)

**Enhanced with:**
```csharp
public class Appointment : AggregateRoot
{
    // Factory method
    public static Appointment Create(...)
    
    // Business methods
    public void Confirm(...)
    public void Cancel(string reason, ...)
    public void Start(...)
    public void Complete(...)
    public void Reschedule(...)
    
    // Query methods
    public bool IsUpcoming()
    public bool ConflictsWith(DateTime otherStart, DateTime otherEnd)
}
```

### 3. DoctorScheduleException (Entity)

**Enhanced with:**
```csharp
public class DoctorScheduleException : BaseEntity
{
    // Factory methods for different exception types
    public static DoctorScheduleException CreateUnavailable(...)
    public static DoctorScheduleException CreateCustomHours(...)
    public static DoctorScheduleException CreateVacation(...)
    public static DoctorScheduleException CreateHoliday(...)
    
    // Business methods
    public void UpdateReason(...)
    public void UpdateCustomHours(...)
    
    // Query methods
    public bool IsAvailable()
    public bool HasCustomWorkingHours()
}
```

## Business Rules Implementation

### 1. Entity-Level Validation
```csharp
public override List<string> ValidateBusinessRules()
{
    var errors = new List<string>();
    
    if (StartTime >= EndTime)
        errors.Add("Start time must be before end time");
        
    return errors;
}
```

### 2. Factory Method Validation
```csharp
public static DoctorSchedule Create(...)
{
    // Validate before creating
    ValidateScheduleTimes(startTime, endTime);
    ValidateSlotDuration(slotDurationMinutes);
    
    // Create and configure
    var schedule = new DoctorSchedule();
    // ... set properties
    
    // Raise domain event
    schedule.AddDomainEvent(new DoctorScheduleCreatedEvent(...));
    
    return schedule;
}
```

### 3. Business Method Validation
```csharp
public void Cancel(string reason, Guid? cancelledBy = null)
{
    if (Status == AppointmentStatus.Completed)
        throw new InvalidOperationException("Cannot cancel completed appointment");
        
    // Apply changes
    Status = AppointmentStatus.Cancelled;
    CancellationReason = reason;
    
    // Raise event
    AddDomainEvent(new AppointmentCancelledEvent(...));
}
```

## Data Integrity Features

### 1. Soft Delete
```csharp
// Instead of hard delete
entity.SoftDelete(userId);

// Check if deleted
if (!entity.IsDeleted) { /* process */ }

// Restore if needed
entity.Restore(userId);
```

### 2. Audit Trail
```csharp
// Automatic audit tracking
Console.WriteLine($"Created by: {entity.CreatedBy} at {entity.DateAdded}");
Console.WriteLine($"Modified by: {entity.UpdatedBy} at {entity.DateUpdated}");
```

### 3. Optimistic Concurrency
```csharp
// Version field automatically managed by EF Core
// Prevents lost update problems in concurrent scenarios
```

## Domain Events Usage

### 1. Event Publishing
```csharp
// In application service
var appointment = Appointment.Create(...);
await _repository.AddAsync(appointment);

// Publish events after successful persistence
foreach (var domainEvent in appointment.DomainEvents)
{
    await _mediator.Publish(domainEvent);
}
appointment.ClearDomainEvents();
```

### 2. Event Handlers
```csharp
public class AppointmentCreatedEventHandler : INotificationHandler<AppointmentCreatedEvent>
{
    public async Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Send confirmation email
        // Update calendar
        // Notify doctor
    }
}
```

## Benefits Achieved

### 1. **Maintainability**
- Consistent structure across all entities
- Centralized business logic
- Clear separation of concerns

### 2. **Data Integrity**
- Encapsulated state changes
- Built-in validation
- Audit trail and soft delete

### 3. **Testability**
- Pure domain logic without infrastructure dependencies
- Factory methods enable easy test data creation
- Business rules can be unit tested

### 4. **Scalability**
- Domain events enable loose coupling
- Easy to add new features without breaking existing code
- Foundation for microservices if needed

### 5. **Business Alignment**
- Domain model reflects business concepts
- Rich behavior in entities
- Clear business rules implementation

This DDD implementation provides a solid foundation for the VoxMed scheduling system while maintaining flexibility for future enhancements.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VoxMed.Domain.Common;

namespace VoxMed.Domain.Entities;

/// <summary>
/// Represents exceptions to the regular doctor schedule (holidays, special hours, etc.)
/// This is an entity that belongs to the DoctorSchedule aggregate
/// </summary>
public class DoctorScheduleException : BaseEntity
{
    // Remove inherited properties
    // [Key] public Guid Id { get; set; } - inherited from BaseEntity

    [Required]
    public Guid DoctorId { get; private set; }

    /// <summary>
    /// Specific date for this exception
    /// </summary>
    [Required]
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Type of exception
    /// </summary>
    [Required]
    public ScheduleExceptionType ExceptionType { get; private set; }

    /// <summary>
    /// Start time for this exception (null if unavailable all day)
    /// </summary>
    public TimeOnly? StartTime { get; private set; }

    /// <summary>
    /// End time for this exception (null if unavailable all day)
    /// </summary>
    public TimeOnly? EndTime { get; private set; }

    /// <summary>
    /// Duration of each appointment slot in minutes (for custom hours)
    /// </summary>
    [Range(5, 240)]
    public int? SlotDurationMinutes { get; private set; }

    /// <summary>
    /// Reason for the exception
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; private set; }

    /// <summary>
    /// Whether this exception is recurring (e.g., every Christmas)
    /// </summary>
    public bool IsRecurring { get; private set; } = false;

    // Remove CreatedAt and UpdatedAt as they're inherited as DateAdded and DateUpdated

    // Navigation properties
    public virtual DoctorSchedule? Schedule { get; private set; }

    // Private constructor for EF Core
    private DoctorScheduleException() { }

    // Factory method for creating unavailable exception
    public static DoctorScheduleException CreateUnavailable(
        Guid doctorId,
        DateOnly date,
        string? reason = null,
        bool isRecurring = false,
        Guid? createdBy = null)
    {
        var exception = new DoctorScheduleException();
        
        exception.DoctorId = doctorId;
        exception.Date = date;
        exception.ExceptionType = ScheduleExceptionType.Unavailable;
        exception.Reason = reason;
        exception.IsRecurring = isRecurring;
        
        exception.SetCreationAuditInfo(createdBy);

        return exception;
    }

    // Factory method for creating custom hours exception
    public static DoctorScheduleException CreateCustomHours(
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        int? slotDurationMinutes = null,
        string? reason = null,
        bool isRecurring = false,
        Guid? createdBy = null)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        var exception = new DoctorScheduleException();
        
        exception.DoctorId = doctorId;
        exception.Date = date;
        exception.ExceptionType = ScheduleExceptionType.CustomHours;
        exception.StartTime = startTime;
        exception.EndTime = endTime;
        exception.SlotDurationMinutes = slotDurationMinutes;
        exception.Reason = reason;
        exception.IsRecurring = isRecurring;
        
        exception.SetCreationAuditInfo(createdBy);

        return exception;
    }

    // Factory method for vacation
    public static DoctorScheduleException CreateVacation(
        Guid doctorId,
        DateOnly date,
        string? reason = null,
        bool isRecurring = false,
        Guid? createdBy = null)
    {
        var exception = new DoctorScheduleException();
        
        exception.DoctorId = doctorId;
        exception.Date = date;
        exception.ExceptionType = ScheduleExceptionType.Vacation;
        exception.Reason = reason ?? "Vacation";
        exception.IsRecurring = isRecurring;
        
        exception.SetCreationAuditInfo(createdBy);

        return exception;
    }

    // Factory method for holiday
    public static DoctorScheduleException CreateHoliday(
        Guid doctorId,
        DateOnly date,
        string? reason = null,
        bool isRecurring = false,
        Guid? createdBy = null)
    {
        var exception = new DoctorScheduleException();
        
        exception.DoctorId = doctorId;
        exception.Date = date;
        exception.ExceptionType = ScheduleExceptionType.Holiday;
        exception.Reason = reason ?? "Holiday";
        exception.IsRecurring = isRecurring;
        
        exception.SetCreationAuditInfo(createdBy);

        return exception;
    }

    // Business methods
    public void UpdateReason(string? reason, Guid? updatedBy = null)
    {
        Reason = reason;
        UpdateAuditInfo(updatedBy);
    }

    public void UpdateCustomHours(
        TimeOnly startTime,
        TimeOnly endTime,
        int? slotDurationMinutes = null,
        Guid? updatedBy = null)
    {
        if (ExceptionType != ScheduleExceptionType.CustomHours)
            throw new InvalidOperationException("Can only update custom hours for custom hours exceptions");

        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        UpdateAuditInfo(updatedBy);
    }

    // Query methods
    public bool IsAvailable()
    {
        return ExceptionType != ScheduleExceptionType.Unavailable &&
               ExceptionType != ScheduleExceptionType.Vacation &&
               ExceptionType != ScheduleExceptionType.Holiday;
    }

    public bool HasCustomWorkingHours()
    {
        return ExceptionType == ScheduleExceptionType.CustomHours &&
               StartTime.HasValue && EndTime.HasValue;
    }
}

/// <summary>
/// Types of schedule exceptions
/// </summary>
public enum ScheduleExceptionType
{
    /// <summary>
    /// Doctor is not available this day
    /// </summary>
    Unavailable = 1,

    /// <summary>
    /// Doctor has custom hours this day
    /// </summary>
    CustomHours = 2,

    /// <summary>
    /// Doctor is on vacation
    /// </summary>
    Vacation = 3,

    /// <summary>
    /// Doctor is attending a conference/training
    /// </summary>
    Conference = 4,

    /// <summary>
    /// Emergency or sick leave
    /// </summary>
    Emergency = 5,

    /// <summary>
    /// Public holiday
    /// </summary>
    Holiday = 6
}

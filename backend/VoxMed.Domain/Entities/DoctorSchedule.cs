using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VoxMed.Domain.Common;
using VoxMed.Domain.Events;

namespace VoxMed.Domain.Entities;

/// <summary>
/// Represents a doctor's working schedule for a specific day
/// This is an aggregate root that manages doctor availability
/// </summary>
public class DoctorSchedule : AggregateRoot
{
    // Remove the base properties as they're now inherited
    // [Key] - inherited from BaseEntity
    // public Guid Id { get; set; } - inherited from BaseEntity

    [Required]
    public Guid DoctorId { get; private set; }

    /// <summary>
    /// Day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
    /// </summary>
    [Required]
    [Range(0, 6)]
    public DayOfWeek DayOfWeek { get; private set; }

    /// <summary>
    /// Start time for the working day
    /// </summary>
    [Required]
    public TimeOnly StartTime { get; private set; }

    /// <summary>
    /// End time for the working day
    /// </summary>
    [Required]
    public TimeOnly EndTime { get; private set; }

    /// <summary>
    /// Duration of each appointment slot in minutes
    /// </summary>
    [Required]
    [Range(5, 240)] // 5 minutes to 4 hours
    public int SlotDurationMinutes { get; private set; } = 30;

    /// <summary>
    /// Break time between appointments in minutes
    /// </summary>
    [Range(0, 60)]
    public int BreakDurationMinutes { get; private set; } = 5;

    /// <summary>
    /// Whether this schedule is currently active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Start date when this schedule becomes effective
    /// </summary>
    public DateOnly? EffectiveFrom { get; private set; }

    /// <summary>
    /// End date when this schedule expires (null = indefinite)
    /// </summary>
    public DateOnly? EffectiveTo { get; private set; }

    // Removed CreatedAt and UpdatedAt as they're inherited as DateAdded and DateUpdated

    // Navigation properties
    public virtual ICollection<DoctorScheduleException> Exceptions { get; private set; } = new List<DoctorScheduleException>();
    public virtual ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();

    // Private constructor for EF Core
    private DoctorSchedule() { }

    // Factory method for creating a new schedule
    public static DoctorSchedule Create(
        Guid doctorId, 
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes = 30,
        int breakDurationMinutes = 5,
        DateOnly? effectiveFrom = null,
        DateOnly? effectiveTo = null,
        Guid? createdBy = null)
    {
        var schedule = new DoctorSchedule();
        
        // Validate business rules
        schedule.ValidateScheduleTimes(startTime, endTime);
        schedule.ValidateSlotDuration(slotDurationMinutes);
        schedule.ValidateBreakDuration(breakDurationMinutes);
        schedule.ValidateEffectiveDates(effectiveFrom, effectiveTo);

        schedule.DoctorId = doctorId;
        schedule.DayOfWeek = dayOfWeek;
        schedule.StartTime = startTime;
        schedule.EndTime = endTime;
        schedule.SlotDurationMinutes = slotDurationMinutes;
        schedule.BreakDurationMinutes = breakDurationMinutes;
        schedule.EffectiveFrom = effectiveFrom;
        schedule.EffectiveTo = effectiveTo;
        schedule.IsActive = true;
        
        schedule.SetCreationAuditInfo(createdBy);

        // Raise domain event
        schedule.AddDomainEvent(new DoctorScheduleCreatedEvent(schedule.Id, doctorId, dayOfWeek));

        return schedule;
    }

    // Business methods
    public void UpdateSchedule(
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes,
        int breakDurationMinutes,
        Guid? updatedBy = null)
    {
        ValidateScheduleTimes(startTime, endTime);
        ValidateSlotDuration(slotDurationMinutes);
        ValidateBreakDuration(breakDurationMinutes);

        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        BreakDurationMinutes = breakDurationMinutes;
        
        UpdateAuditInfo(updatedBy);

        AddDomainEvent(new DoctorScheduleUpdatedEvent(Id, DoctorId));
    }

    public void Activate(Guid? updatedBy = null)
    {
        IsActive = true;
        UpdateAuditInfo(updatedBy);
        
        AddDomainEvent(new DoctorScheduleActivatedEvent(Id, DoctorId));
    }

    public void Deactivate(Guid? updatedBy = null)
    {
        IsActive = false;
        UpdateAuditInfo(updatedBy);
        
        AddDomainEvent(new DoctorScheduleDeactivatedEvent(Id, DoctorId));
    }

    public void SetEffectiveDates(DateOnly? effectiveFrom, DateOnly? effectiveTo, Guid? updatedBy = null)
    {
        ValidateEffectiveDates(effectiveFrom, effectiveTo);
        
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        UpdateAuditInfo(updatedBy);
    }

    // Business rule validation
    private void ValidateScheduleTimes(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");
    }

    private void ValidateSlotDuration(int slotDurationMinutes)
    {
        if (slotDurationMinutes < 5 || slotDurationMinutes > 240)
            throw new ArgumentException("Slot duration must be between 5 and 240 minutes");
    }

    private void ValidateBreakDuration(int breakDurationMinutes)
    {
        if (breakDurationMinutes < 0 || breakDurationMinutes > 60)
            throw new ArgumentException("Break duration must be between 0 and 60 minutes");
    }

    private void ValidateEffectiveDates(DateOnly? effectiveFrom, DateOnly? effectiveTo)
    {
        if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveFrom.Value > effectiveTo.Value)
            throw new ArgumentException("Effective from date must be before effective to date");
    }

    public override List<string> ValidateBusinessRules()
    {
        var errors = new List<string>();

        if (StartTime >= EndTime)
            errors.Add("Start time must be before end time");

        if (SlotDurationMinutes < 5 || SlotDurationMinutes > 240)
            errors.Add("Slot duration must be between 5 and 240 minutes");

        if (BreakDurationMinutes < 0 || BreakDurationMinutes > 60)
            errors.Add("Break duration must be between 0 and 60 minutes");

        if (EffectiveFrom.HasValue && EffectiveTo.HasValue && EffectiveFrom.Value > EffectiveTo.Value)
            errors.Add("Effective from date must be before effective to date");

        return errors;
    }

    // Query methods
    public bool IsEffectiveOn(DateOnly date)
    {
        return IsActive &&
               (!EffectiveFrom.HasValue || date >= EffectiveFrom.Value) &&
               (!EffectiveTo.HasValue || date <= EffectiveTo.Value);
    }

    public int GetTotalSlotsPerDay()
    {
        var totalMinutes = (EndTime - StartTime).TotalMinutes;
        var slotWithBreak = SlotDurationMinutes + BreakDurationMinutes;
        return (int)(totalMinutes / slotWithBreak);
    }
}

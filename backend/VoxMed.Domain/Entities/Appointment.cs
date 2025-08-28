using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VoxMed.Domain.Common;
using VoxMed.Domain.Events;

namespace VoxMed.Domain.Entities;

/// <summary>
/// Represents an appointment between a doctor and patient
/// This is an aggregate root that manages the appointment lifecycle
/// </summary>
public class Appointment : AggregateRoot
{
    // Remove inherited properties
    // [Key] public Guid Id { get; set; } - inherited from BaseEntity

    [Required]
    public Guid DoctorId { get; private set; }

    [Required]
    public Guid PatientId { get; private set; }

    /// <summary>
    /// Reference to the doctor's schedule (optional, for tracking)
    /// </summary>
    public Guid? ScheduleId { get; private set; }

    /// <summary>
    /// Date and time of the appointment
    /// </summary>
    [Required]
    public DateTime AppointmentDateTime { get; private set; }

    /// <summary>
    /// Duration of the appointment in minutes
    /// </summary>
    [Required]
    [Range(5, 240)]
    public int DurationMinutes { get; private set; } = 30;

    /// <summary>
    /// Current status of the appointment
    /// </summary>
    [Required]
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;

    /// <summary>
    /// Type/category of appointment
    /// </summary>
    [Required]
    public AppointmentType Type { get; private set; } = AppointmentType.Consultation;

    /// <summary>
    /// Reason for the appointment
    /// </summary>
    [MaxLength(1000)]
    public string? Reason { get; private set; }

    /// <summary>
    /// Notes from the doctor
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; private set; }

    /// <summary>
    /// Patient's symptoms or chief complaint
    /// </summary>
    [MaxLength(1000)]
    public string? Symptoms { get; private set; }

    /// <summary>
    /// Diagnosis given during the appointment
    /// </summary>
    [MaxLength(1000)]
    public string? Diagnosis { get; private set; }

    /// <summary>
    /// Treatment prescribed
    /// </summary>
    [MaxLength(2000)]
    public string? Treatment { get; private set; }

    /// <summary>
    /// Whether this appointment allows online consultation
    /// </summary>
    public bool IsOnline { get; private set; } = false;

    /// <summary>
    /// Room number or online meeting link
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; private set; }

    // Remove CreatedAt and UpdatedAt as they're inherited as DateAdded and DateUpdated

    /// <summary>
    /// When the appointment was confirmed by the patient
    /// </summary>
    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>
    /// When the appointment was cancelled
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// Reason for cancellation
    /// </summary>
    [MaxLength(500)]
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// When the appointment was completed
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties will be added when we have the User entities
    // public virtual ApplicationUser Doctor { get; set; } = null!;
    // public virtual ApplicationUser Patient { get; set; } = null!;
    public virtual DoctorSchedule? Schedule { get; private set; }

    // Private constructor for EF Core
    private Appointment() { }

    // Factory method for creating a new appointment
    public static Appointment Create(
        Guid doctorId,
        Guid patientId,
        DateTime appointmentDateTime,
        int durationMinutes = 30,
        AppointmentType type = AppointmentType.Consultation,
        string? reason = null,
        bool isOnline = false,
        string? location = null,
        Guid? scheduleId = null,
        Guid? createdBy = null)
    {
        var appointment = new Appointment();
        
        // Validate business rules
        appointment.ValidateAppointmentDateTime(appointmentDateTime);
        appointment.ValidateDuration(durationMinutes);

        appointment.DoctorId = doctorId;
        appointment.PatientId = patientId;
        appointment.AppointmentDateTime = appointmentDateTime;
        appointment.DurationMinutes = durationMinutes;
        appointment.Type = type;
        appointment.Reason = reason;
        appointment.IsOnline = isOnline;
        appointment.Location = location;
        appointment.ScheduleId = scheduleId;
        appointment.Status = AppointmentStatus.Scheduled;
        
        appointment.SetCreationAuditInfo(createdBy);

        // Raise domain event
        appointment.AddDomainEvent(new AppointmentCreatedEvent(
            appointment.Id, doctorId, patientId, appointmentDateTime));

        return appointment;
    }

    // Business methods
    public void Confirm(Guid? confirmedBy = null)
    {
        if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot confirm appointment in {Status} status");

        Status = AppointmentStatus.Scheduled;
        ConfirmedAt = DateTime.UtcNow;
        UpdateAuditInfo(confirmedBy);

        AddDomainEvent(new AppointmentConfirmedEvent(Id, DoctorId, PatientId));
    }

    public void Cancel(string reason, Guid? cancelledBy = null)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel appointment in {Status} status");

        Status = AppointmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdateAuditInfo(cancelledBy);

        AddDomainEvent(new AppointmentCancelledEvent(Id, DoctorId, PatientId, reason));
    }

    public void Start(Guid? startedBy = null)
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start appointment in {Status} status");

        Status = AppointmentStatus.InProgress;
        UpdateAuditInfo(startedBy);

        AddDomainEvent(new AppointmentStartedEvent(Id, DoctorId, PatientId));
    }

    public void Complete(
        string? diagnosis = null,
        string? treatment = null,
        string? notes = null,
        Guid? completedBy = null)
    {
        if (Status != AppointmentStatus.InProgress && Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot complete appointment in {Status} status");

        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Diagnosis = diagnosis;
        Treatment = treatment;
        Notes = notes;
        UpdateAuditInfo(completedBy);

        AddDomainEvent(new AppointmentCompletedEvent(Id, DoctorId, PatientId));
    }

    public void MarkAsNoShow(Guid? markedBy = null)
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot mark as no-show appointment in {Status} status");

        Status = AppointmentStatus.NoShow;
        UpdateAuditInfo(markedBy);

        AddDomainEvent(new AppointmentNoShowEvent(Id, DoctorId, PatientId));
    }

    public void Reschedule(DateTime newDateTime, Guid? rescheduledBy = null)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot reschedule appointment in {Status} status");

        ValidateAppointmentDateTime(newDateTime);

        var oldDateTime = AppointmentDateTime;
        AppointmentDateTime = newDateTime;
        Status = AppointmentStatus.Rescheduled;
        UpdateAuditInfo(rescheduledBy);

        AddDomainEvent(new AppointmentRescheduledEvent(Id, DoctorId, PatientId, oldDateTime, newDateTime));
    }

    public void AddSymptoms(string symptoms, Guid? addedBy = null)
    {
        Symptoms = symptoms;
        UpdateAuditInfo(addedBy);
    }

    public void UpdateLocation(string? location, Guid? updatedBy = null)
    {
        Location = location;
        UpdateAuditInfo(updatedBy);
    }

    // Business rule validation
    private void ValidateAppointmentDateTime(DateTime appointmentDateTime)
    {
        if (appointmentDateTime <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future");
    }

    private void ValidateDuration(int durationMinutes)
    {
        if (durationMinutes < 5 || durationMinutes > 240)
            throw new ArgumentException("Duration must be between 5 and 240 minutes");
    }

    public override List<string> ValidateBusinessRules()
    {
        var errors = new List<string>();

        if (AppointmentDateTime <= DateTime.UtcNow)
            errors.Add("Appointment date must be in the future");

        if (DurationMinutes < 5 || DurationMinutes > 240)
            errors.Add("Duration must be between 5 and 240 minutes");

        if (DoctorId == PatientId)
            errors.Add("Doctor and patient cannot be the same person");

        if (Status == AppointmentStatus.Cancelled && string.IsNullOrEmpty(CancellationReason))
            errors.Add("Cancellation reason is required for cancelled appointments");

        return errors;
    }

    // Query methods
    public bool IsUpcoming()
    {
        return AppointmentDateTime > DateTime.UtcNow &&
               (Status == AppointmentStatus.Scheduled || Status == AppointmentStatus.Pending);
    }

    public bool IsToday()
    {
        return AppointmentDateTime.Date == DateTime.UtcNow.Date;
    }

    public DateTime EndDateTime => AppointmentDateTime.AddMinutes(DurationMinutes);

    public bool ConflictsWith(DateTime otherStart, DateTime otherEnd)
    {
        return AppointmentDateTime < otherEnd && EndDateTime > otherStart;
    }
}

/// <summary>
/// Status of an appointment
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Appointment is scheduled and confirmed
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// Appointment is pending confirmation
    /// </summary>
    Pending = 2,

    /// <summary>
    /// Appointment was cancelled
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Appointment was completed
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Patient didn't show up
    /// </summary>
    NoShow = 5,

    /// <summary>
    /// Appointment is currently in progress
    /// </summary>
    InProgress = 6,

    /// <summary>
    /// Appointment needs to be rescheduled
    /// </summary>
    Rescheduled = 7
}

/// <summary>
/// Type of appointment
/// </summary>
public enum AppointmentType
{
    /// <summary>
    /// Regular consultation
    /// </summary>
    Consultation = 1,

    /// <summary>
    /// Follow-up appointment
    /// </summary>
    FollowUp = 2,

    /// <summary>
    /// Emergency appointment
    /// </summary>
    Emergency = 3,

    /// <summary>
    /// Routine check-up
    /// </summary>
    CheckUp = 4,

    /// <summary>
    /// Vaccination appointment
    /// </summary>
    Vaccination = 5,

    /// <summary>
    /// Diagnostic procedure
    /// </summary>
    Diagnostic = 6,

    /// <summary>
    /// Surgery or procedure
    /// </summary>
    Procedure = 7,

    /// <summary>
    /// Online/telemedicine consultation
    /// </summary>
    Telemedicine = 8
}

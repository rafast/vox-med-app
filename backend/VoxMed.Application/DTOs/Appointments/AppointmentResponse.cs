using VoxMed.Domain.Entities;

namespace VoxMed.Application.DTOs.Appointments;

/// <summary>
/// Response DTO for appointment data
/// </summary>
public class AppointmentResponse
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public AppointmentType Type { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public bool IsOnline { get; set; }
    public string? Location { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }

    // Additional computed properties
    public string StatusDisplayName => Status.ToString();
    public string TypeDisplayName => Type.ToString();
    public DateTime AppointmentEndDateTime => AppointmentDateTime.AddMinutes(DurationMinutes);
    public bool IsUpcoming => AppointmentDateTime > DateTime.UtcNow && Status == AppointmentStatus.Scheduled;
    public bool IsPast => AppointmentDateTime < DateTime.UtcNow;
    public bool CanBeCancelled => Status == AppointmentStatus.Scheduled && AppointmentDateTime > DateTime.UtcNow.AddHours(1);
    public bool CanBeRescheduled => Status == AppointmentStatus.Scheduled && AppointmentDateTime > DateTime.UtcNow.AddHours(2);
}

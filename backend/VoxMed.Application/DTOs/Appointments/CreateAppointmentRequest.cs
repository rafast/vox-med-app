using System.ComponentModel.DataAnnotations;
using VoxMed.Domain.Entities;

namespace VoxMed.Application.DTOs.Appointments;

/// <summary>
/// Request DTO for creating a new appointment
/// </summary>
public class CreateAppointmentRequest
{
    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    public Guid? ScheduleId { get; set; }

    [Required]
    public DateTime AppointmentDateTime { get; set; }

    [Range(5, 240)]
    public int DurationMinutes { get; set; } = 30;

    [Required]
    public AppointmentType Type { get; set; } = AppointmentType.Consultation;

    [MaxLength(1000)]
    public string? Reason { get; set; }

    public bool IsOnline { get; set; } = false;

    [MaxLength(200)]
    public string? Location { get; set; }
}

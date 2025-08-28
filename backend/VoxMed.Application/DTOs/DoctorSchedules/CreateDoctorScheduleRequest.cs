using System.ComponentModel.DataAnnotations;

namespace VoxMed.Application.DTOs.DoctorSchedules;

public class CreateDoctorScheduleRequest
{
    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    [Range(15, 120)]
    public int SlotDurationMinutes { get; set; }

    public DateOnly? EffectiveFrom { get; set; }
    
    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;
}

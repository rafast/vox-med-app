using MediatR;
using VoxMed.Application.DTOs.DoctorSchedules;

namespace VoxMed.Application.Commands.DoctorSchedules;

public class CreateDoctorScheduleCommand : IRequest<DoctorScheduleResponse>
{
    public Guid DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CreatedBy { get; set; }

    public CreateDoctorScheduleCommand(CreateDoctorScheduleRequest request, Guid? createdBy = null)
    {
        DoctorId = request.DoctorId;
        DayOfWeek = request.DayOfWeek;
        StartTime = request.StartTime;
        EndTime = request.EndTime;
        SlotDurationMinutes = request.SlotDurationMinutes;
        EffectiveFrom = request.EffectiveFrom;
        EffectiveTo = request.EffectiveTo;
        IsActive = request.IsActive;
        CreatedBy = createdBy;
    }
}

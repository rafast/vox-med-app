using MediatR;
using VoxMed.Application.DTOs.Appointments;

namespace VoxMed.Application.Commands.Appointments;

public class CancelAppointmentCommand : IRequest<AppointmentResponse>
{
    public Guid Id { get; set; }
    public string? CancellationReason { get; set; }

    public CancelAppointmentCommand(Guid id, string? cancellationReason = null)
    {
        Id = id;
        CancellationReason = cancellationReason;
    }
}

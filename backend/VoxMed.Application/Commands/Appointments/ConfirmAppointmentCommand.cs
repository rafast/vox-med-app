using MediatR;
using VoxMed.Application.DTOs.Appointments;

namespace VoxMed.Application.Commands.Appointments;

public class ConfirmAppointmentCommand : IRequest<AppointmentResponse>
{
    public Guid Id { get; set; }

    public ConfirmAppointmentCommand(Guid id)
    {
        Id = id;
    }
}

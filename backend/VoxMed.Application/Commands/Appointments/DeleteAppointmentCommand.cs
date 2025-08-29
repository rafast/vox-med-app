using MediatR;

namespace VoxMed.Application.Commands.Appointments;

public class DeleteAppointmentCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteAppointmentCommand(Guid id)
    {
        Id = id;
    }
}

using MediatR;
using VoxMed.Application.DTOs.Appointments;

namespace VoxMed.Application.Commands.Appointments;

public class CreateAppointmentCommand : IRequest<AppointmentResponse>
{
    public CreateAppointmentRequest Request { get; set; }

    public CreateAppointmentCommand(CreateAppointmentRequest request)
    {
        Request = request;
    }
}

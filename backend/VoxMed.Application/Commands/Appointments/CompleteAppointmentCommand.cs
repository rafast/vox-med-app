using MediatR;
using VoxMed.Application.DTOs.Appointments;

namespace VoxMed.Application.Commands.Appointments;

public class CompleteAppointmentCommand : IRequest<AppointmentResponse>
{
    public Guid Id { get; set; }
    public string? Notes { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }

    public CompleteAppointmentCommand(Guid id, string? notes = null, string? diagnosis = null, string? treatment = null)
    {
        Id = id;
        Notes = notes;
        Diagnosis = diagnosis;
        Treatment = treatment;
    }
}

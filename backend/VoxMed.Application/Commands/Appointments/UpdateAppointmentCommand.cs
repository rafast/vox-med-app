using MediatR;
using VoxMed.Application.DTOs.Appointments;
using VoxMed.Domain.Entities;

namespace VoxMed.Application.Commands.Appointments;

public class UpdateAppointmentCommand : IRequest<AppointmentResponse>
{
    public Guid Id { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentType Type { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }

    public UpdateAppointmentCommand(Guid id, DateTime appointmentDateTime, int durationMinutes, 
        AppointmentType type, string? reason = null, string? notes = null, string? symptoms = null, 
        string? diagnosis = null, string? treatment = null, string? prescription = null)
    {
        Id = id;
        AppointmentDateTime = appointmentDateTime;
        DurationMinutes = durationMinutes;
        Type = type;
        Reason = reason;
        Notes = notes;
        Symptoms = symptoms;
        Diagnosis = diagnosis;
        Treatment = treatment;
        Prescription = prescription;
    }
}

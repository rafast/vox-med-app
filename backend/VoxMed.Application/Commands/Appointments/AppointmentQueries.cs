using MediatR;
using VoxMed.Application.DTOs.Appointments;
using VoxMed.Domain.Entities;

namespace VoxMed.Application.Commands.Appointments;

// Query commands for appointments
public record GetAllAppointmentsQuery() : IRequest<List<AppointmentResponse>>;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentResponse?>;

public record GetDoctorAppointmentsQuery(Guid DoctorId) : IRequest<List<AppointmentResponse>>;

public record GetPatientAppointmentsQuery(Guid PatientId) : IRequest<List<AppointmentResponse>>;

public record GetAppointmentsByDateRangeQuery(DateTime StartDate, DateTime EndDate) : IRequest<List<AppointmentResponse>>;

public record GetDoctorAppointmentsByDateRangeQuery(Guid DoctorId, DateTime StartDate, DateTime EndDate) : IRequest<List<AppointmentResponse>>;

public record GetUpcomingDoctorAppointmentsQuery(Guid DoctorId) : IRequest<List<AppointmentResponse>>;

public record GetUpcomingPatientAppointmentsQuery(Guid PatientId) : IRequest<List<AppointmentResponse>>;

public record GetAppointmentsByStatusQuery(AppointmentStatus Status) : IRequest<List<AppointmentResponse>>;

public record GetTodayAppointmentsQuery(Guid DoctorId) : IRequest<List<AppointmentResponse>>;

public record GetPendingAppointmentsQuery() : IRequest<List<AppointmentResponse>>;

public record GetAvailableTimeSlotsQuery(Guid DoctorId, DateTime Date) : IRequest<List<TimeSlotResponse>>;

public record TimeSlotResponse(DateTime StartTime, DateTime EndTime, int DurationMinutes, bool IsAvailable);

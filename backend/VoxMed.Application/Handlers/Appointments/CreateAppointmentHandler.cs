using AutoMapper;
using MediatR;
using VoxMed.Application.Commands.Appointments;
using VoxMed.Application.DTOs.Appointments;
using VoxMed.Application.Interfaces.Repositories;
using VoxMed.Domain.Entities;

namespace VoxMed.Application.Handlers.Appointments;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorScheduleRepository _scheduleRepository;
    private readonly IMapper _mapper;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorScheduleRepository scheduleRepository,
        IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _scheduleRepository = scheduleRepository;
        _mapper = mapper;
    }

    public async Task<AppointmentResponse> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointmentRequest = request.Request;

        // Check for conflicting appointments
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            appointmentRequest.DoctorId,
            appointmentRequest.AppointmentDateTime,
            appointmentRequest.DurationMinutes);

        if (hasConflict)
        {
            throw new InvalidOperationException(
                $"Doctor already has a conflicting appointment at {appointmentRequest.AppointmentDateTime:yyyy-MM-dd HH:mm}");
        }

        // Validate doctor availability (check if appointment time is within doctor's schedule)
        if (appointmentRequest.ScheduleId.HasValue)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(appointmentRequest.ScheduleId.Value, cancellationToken);
            if (schedule == null)
            {
                throw new InvalidOperationException("Schedule not found");
            }

            // Validate if the appointment time falls within the schedule
            var dayOfWeek = (int)appointmentRequest.AppointmentDateTime.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // Convert Sunday from 0 to 7

            if ((int)schedule.DayOfWeek != dayOfWeek)
            {
                throw new InvalidOperationException("Appointment date does not match the schedule day");
            }

            var appointmentTime = TimeOnly.FromTimeSpan(appointmentRequest.AppointmentDateTime.TimeOfDay);
            if (appointmentTime < schedule.StartTime || appointmentTime >= schedule.EndTime)
            {
                throw new InvalidOperationException("Appointment time is outside doctor's available hours");
            }
        }

        // Create the appointment
        var appointment = Appointment.Create(
            appointmentRequest.DoctorId,
            appointmentRequest.PatientId,
            appointmentRequest.AppointmentDateTime,
            appointmentRequest.DurationMinutes,
            appointmentRequest.Type,
            appointmentRequest.Reason,
            appointmentRequest.IsOnline,
            appointmentRequest.Location,
            appointmentRequest.ScheduleId);

        var savedAppointment = await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return _mapper.Map<AppointmentResponse>(savedAppointment);
    }
}

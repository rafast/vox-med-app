using AutoMapper;
using MediatR;
using VoxMed.Application.Commands.Appointments;
using VoxMed.Application.DTOs.Appointments;
using VoxMed.Application.Interfaces.Repositories;

namespace VoxMed.Application.Handlers.Appointments;

public class GetAllAppointmentsHandler : IRequestHandler<GetAllAppointmentsQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetAllAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetAllAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentResponse?>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetAppointmentByIdHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<AppointmentResponse?> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken);
        return appointment == null ? null : _mapper.Map<AppointmentResponse>(appointment);
    }
}

public class GetDoctorAppointmentsHandler : IRequestHandler<GetDoctorAppointmentsQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetDoctorAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetDoctorAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetDoctorAppointmentsAsync(request.DoctorId);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetPatientAppointmentsHandler : IRequestHandler<GetPatientAppointmentsQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetPatientAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetPatientAppointmentsAsync(request.PatientId);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetAppointmentsByDateRangeHandler : IRequestHandler<GetAppointmentsByDateRangeQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetAppointmentsByDateRangeHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetAppointmentsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByDateRangeAsync(request.StartDate, request.EndDate);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetDoctorAppointmentsByDateRangeHandler : IRequestHandler<GetDoctorAppointmentsByDateRangeQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetDoctorAppointmentsByDateRangeHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetDoctorAppointmentsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateRangeAsync(request.DoctorId, request.StartDate, request.EndDate);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetUpcomingDoctorAppointmentsHandler : IRequestHandler<GetUpcomingDoctorAppointmentsQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetUpcomingDoctorAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetUpcomingDoctorAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetUpcomingDoctorAppointmentsAsync(request.DoctorId);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetUpcomingPatientAppointmentsHandler : IRequestHandler<GetUpcomingPatientAppointmentsQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetUpcomingPatientAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetUpcomingPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetUpcomingPatientAppointmentsAsync(request.PatientId);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetAppointmentsByStatusHandler : IRequestHandler<GetAppointmentsByStatusQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetAppointmentsByStatusHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetAppointmentsByStatusQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByStatusAsync(request.Status);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetTodayAppointmentsHandler : IRequestHandler<GetTodayAppointmentsQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetTodayAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetTodayAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetTodayAppointmentsAsync(request.DoctorId);
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetPendingAppointmentsHandler : IRequestHandler<GetPendingAppointmentsQuery, List<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;

    public GetPendingAppointmentsHandler(IAppointmentRepository appointmentRepository, IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
    }

    public async Task<List<AppointmentResponse>> Handle(GetPendingAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetPendingAppointmentsAsync();
        return _mapper.Map<List<AppointmentResponse>>(appointments);
    }
}

public class GetAvailableTimeSlotsHandler : IRequestHandler<GetAvailableTimeSlotsQuery, List<TimeSlotResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAvailableTimeSlotsHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<TimeSlotResponse>> Handle(GetAvailableTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        var timeSlots = await _appointmentRepository.GetAvailableTimeSlotsAsync(request.DoctorId, request.Date);
        return timeSlots.Select(ts => new TimeSlotResponse(ts.StartTime, ts.EndTime, ts.DurationMinutes, ts.IsAvailable)).ToList();
    }
}

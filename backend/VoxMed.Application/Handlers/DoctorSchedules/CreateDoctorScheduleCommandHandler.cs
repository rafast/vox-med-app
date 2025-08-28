using MediatR;
using AutoMapper;
using VoxMed.Application.Commands.DoctorSchedules;
using VoxMed.Application.DTOs.DoctorSchedules;
using VoxMed.Application.Interfaces.Repositories;
using VoxMed.Domain.Entities;

namespace VoxMed.Application.Handlers.DoctorSchedules;

public class CreateDoctorScheduleCommandHandler : IRequestHandler<CreateDoctorScheduleCommand, DoctorScheduleResponse>
{
    private readonly IDoctorScheduleRepository _doctorScheduleRepository;
    private readonly IMapper _mapper;

    public CreateDoctorScheduleCommandHandler(
        IDoctorScheduleRepository doctorScheduleRepository,
        IMapper mapper)
    {
        _doctorScheduleRepository = doctorScheduleRepository;
        _mapper = mapper;
    }

    public async Task<DoctorScheduleResponse> Handle(CreateDoctorScheduleCommand request, CancellationToken cancellationToken)
    {
        // Check for conflicting schedules
        var hasConflict = await _doctorScheduleRepository.HasConflictingScheduleAsync(
            request.DoctorId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            null,
            cancellationToken);

        if (hasConflict)
        {
            throw new InvalidOperationException($"Doctor already has a conflicting schedule for {request.DayOfWeek}");
        }

        // Create the doctor schedule using the domain factory method
        var doctorSchedule = DoctorSchedule.Create(
            request.DoctorId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            request.SlotDurationMinutes,
            5, // breakDurationMinutes - default 5 minutes
            request.EffectiveFrom,
            request.EffectiveTo,
            request.CreatedBy);

        // Validate business rules
        var validationErrors = doctorSchedule.ValidateBusinessRules();
        if (validationErrors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
        }

        // Save to repository
        await _doctorScheduleRepository.AddAsync(doctorSchedule, cancellationToken);

        // Map to response DTO
        var response = _mapper.Map<DoctorScheduleResponse>(doctorSchedule);
        response.TotalSlotsPerDay = doctorSchedule.GetTotalSlotsPerDay();

        // TODO: Publish domain events (if using event publishing)
        // foreach (var domainEvent in doctorSchedule.DomainEvents)
        // {
        //     await _mediator.Publish(domainEvent, cancellationToken);
        // }
        // doctorSchedule.ClearDomainEvents();

        return response;
    }
}

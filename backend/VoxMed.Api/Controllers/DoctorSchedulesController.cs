using Microsoft.AspNetCore.Mvc;
using VoxMed.Application.DTOs.DoctorSchedules;
using VoxMed.Application.Interfaces.Repositories;
using VoxMed.Domain.Entities;
using AutoMapper;

namespace VoxMed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorSchedulesController : ControllerBase
{
    private readonly IDoctorScheduleRepository _doctorScheduleRepository;
    private readonly IMapper _mapper;

    public DoctorSchedulesController(
        IDoctorScheduleRepository doctorScheduleRepository,
        IMapper mapper)
    {
        _doctorScheduleRepository = doctorScheduleRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<DoctorScheduleResponse>> CreateDoctorSchedule(CreateDoctorScheduleRequest request)
    {
        try
        {
            // Check for conflicting schedules
            var hasConflict = await _doctorScheduleRepository.HasConflictingScheduleAsync(
                request.DoctorId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime);

            if (hasConflict)
            {
                return BadRequest($"Doctor already has a conflicting schedule for {request.DayOfWeek}");
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
                request.EffectiveTo);

            // Validate business rules
            var validationErrors = doctorSchedule.ValidateBusinessRules();
            if (validationErrors.Any())
            {
                return BadRequest($"Validation failed: {string.Join(", ", validationErrors)}");
            }

            // Save to repository
            await _doctorScheduleRepository.AddAsync(doctorSchedule);

            // Map to response DTO
            var response = _mapper.Map<DoctorScheduleResponse>(doctorSchedule);
            response.TotalSlotsPerDay = doctorSchedule.GetTotalSlotsPerDay();

            return CreatedAtAction(nameof(GetDoctorSchedule), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating doctor schedule: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorScheduleResponse>> GetDoctorSchedule(Guid id)
    {
        var schedule = await _doctorScheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        var response = _mapper.Map<DoctorScheduleResponse>(schedule);
        response.TotalSlotsPerDay = schedule.GetTotalSlotsPerDay();

        return Ok(response);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<ActionResult<List<DoctorScheduleResponse>>> GetDoctorSchedules(Guid doctorId)
    {
        var schedules = await _doctorScheduleRepository.GetByDoctorIdAsync(doctorId);
        
        var responses = schedules.Select(schedule =>
        {
            var response = _mapper.Map<DoctorScheduleResponse>(schedule);
            response.TotalSlotsPerDay = schedule.GetTotalSlotsPerDay();
            return response;
        }).ToList();

        return Ok(responses);
    }

    [HttpGet("doctor/{doctorId}/active")]
    public async Task<ActionResult<List<DoctorScheduleResponse>>> GetActiveDoctorSchedules(Guid doctorId)
    {
        var schedules = await _doctorScheduleRepository.GetActiveDoctorSchedulesAsync(doctorId);
        
        var responses = schedules.Select(schedule =>
        {
            var response = _mapper.Map<DoctorScheduleResponse>(schedule);
            response.TotalSlotsPerDay = schedule.GetTotalSlotsPerDay();
            return response;
        }).ToList();

        return Ok(responses);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DoctorScheduleResponse>> UpdateDoctorSchedule(Guid id, UpdateDoctorScheduleRequest request)
    {
        try
        {
            var existingSchedule = await _doctorScheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound();
            }

            // Check for conflicting schedules (excluding current one)
            var hasConflict = await _doctorScheduleRepository.HasConflictingScheduleAsync(
                existingSchedule.DoctorId,
                existingSchedule.DayOfWeek,
                request.StartTime,
                request.EndTime,
                id);

            if (hasConflict)
            {
                return BadRequest($"Doctor already has a conflicting schedule for {existingSchedule.DayOfWeek}");
            }

            // Update the schedule
            existingSchedule.UpdateSchedule(
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes,
                5); // breakDurationMinutes

            // Update effective dates separately
            existingSchedule.SetEffectiveDates(request.EffectiveFrom, request.EffectiveTo);

            if (request.IsActive)
                existingSchedule.Activate();
            else
                existingSchedule.Deactivate();

            // Validate business rules
            var validationErrors = existingSchedule.ValidateBusinessRules();
            if (validationErrors.Any())
            {
                return BadRequest($"Validation failed: {string.Join(", ", validationErrors)}");
            }

            await _doctorScheduleRepository.UpdateAsync(existingSchedule);

            var response = _mapper.Map<DoctorScheduleResponse>(existingSchedule);
            response.TotalSlotsPerDay = existingSchedule.GetTotalSlotsPerDay();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating doctor schedule: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDoctorSchedule(Guid id)
    {
        var schedule = await _doctorScheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        await _doctorScheduleRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<DoctorScheduleResponse>>> GetPaginatedDoctorSchedules(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] bool? isActive = null)
    {
        var schedules = await _doctorScheduleRepository.GetPaginatedAsync(page, pageSize, doctorId, isActive);
        var totalCount = await _doctorScheduleRepository.GetCountAsync(doctorId, isActive);

        var responses = schedules.Select(schedule =>
        {
            var response = _mapper.Map<DoctorScheduleResponse>(schedule);
            response.TotalSlotsPerDay = schedule.GetTotalSlotsPerDay();
            return response;
        }).ToList();

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(responses);
    }
}

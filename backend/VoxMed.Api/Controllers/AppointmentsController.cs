using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxMed.Application.Commands.Appointments;
using VoxMed.Application.DTOs.Appointments;

namespace VoxMed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all appointments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAllAppointments(CancellationToken cancellationToken)
    {
        var query = new GetAllAppointmentsQuery();
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAppointmentByIdQuery(id);
        var appointment = await _mediator.Send(query, cancellationToken);
        
        if (appointment == null)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(appointment);
    }

    /// <summary>
    /// Create a new appointment
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment(
        [FromBody] CreateAppointmentRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAppointmentCommand(request);

            var appointment = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing appointment
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointment(
        Guid id, 
        [FromBody] UpdateAppointmentRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateAppointmentCommand(
                id,
                request.AppointmentDateTime,
                request.DurationMinutes,
                request.Type,
                request.Reason,
                request.Notes,
                request.Symptoms,
                request.Diagnosis,
                request.Treatment);

            var appointment = await _mediator.Send(command, cancellationToken);
            
            if (appointment == null)
                return NotFound($"Appointment with ID {id} not found.");

            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an appointment
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAppointment(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteAppointmentCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
            return NotFound($"Appointment with ID {id} not found.");

        return NoContent();
    }

    /// <summary>
    /// Confirm an appointment
    /// </summary>
    [HttpPatch("{id:guid}/confirm")]
    public async Task<ActionResult<AppointmentResponse>> ConfirmAppointment(Guid id, CancellationToken cancellationToken)
    {
        var command = new ConfirmAppointmentCommand(id);
        var appointment = await _mediator.Send(command, cancellationToken);
        
        if (appointment == null)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(appointment);
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentResponse>> CancelAppointment(
        Guid id, 
        [FromBody] CancelAppointmentRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new CancelAppointmentCommand(id, request.CancellationReason);
        var appointment = await _mediator.Send(command, cancellationToken);
        
        if (appointment == null)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(appointment);
    }

    /// <summary>
    /// Complete an appointment
    /// </summary>
    [HttpPatch("{id:guid}/complete")]
    public async Task<ActionResult<AppointmentResponse>> CompleteAppointment(
        Guid id, 
        [FromBody] CompleteAppointmentRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new CompleteAppointmentCommand(id, request.Notes);
        var appointment = await _mediator.Send(command, cancellationToken);
        
        if (appointment == null)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(appointment);
    }

    /// <summary>
    /// Get doctor's appointments
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetDoctorAppointments(
        Guid doctorId, 
        CancellationToken cancellationToken)
    {
        var query = new GetDoctorAppointmentsQuery(doctorId);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get patient's appointments
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetPatientAppointments(
        Guid patientId, 
        CancellationToken cancellationToken)
    {
        var query = new GetPatientAppointmentsQuery(patientId);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointments by date range
    /// </summary>
    [HttpGet("date-range")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAppointmentsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetAppointmentsByDateRangeQuery(startDate, endDate);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get doctor's appointments by date range
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/date-range")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetDoctorAppointmentsByDateRange(
        Guid doctorId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetDoctorAppointmentsByDateRangeQuery(doctorId, startDate, endDate);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get doctor's upcoming appointments
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/upcoming")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetUpcomingDoctorAppointments(
        Guid doctorId, 
        CancellationToken cancellationToken)
    {
        var query = new GetUpcomingDoctorAppointmentsQuery(doctorId);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get patient's upcoming appointments
    /// </summary>
    [HttpGet("patient/{patientId:guid}/upcoming")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetUpcomingPatientAppointments(
        Guid patientId, 
        CancellationToken cancellationToken)
    {
        var query = new GetUpcomingPatientAppointmentsQuery(patientId);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get appointments by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAppointmentsByStatus(
        string status, 
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<VoxMed.Domain.Entities.AppointmentStatus>(status, true, out var appointmentStatus))
        {
            return BadRequest($"Invalid appointment status: {status}");
        }

        var query = new GetAppointmentsByStatusQuery(appointmentStatus);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get today's appointments for a doctor
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/today")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetTodayAppointments(
        Guid doctorId, 
        CancellationToken cancellationToken)
    {
        var query = new GetTodayAppointmentsQuery(doctorId);
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get pending appointments
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetPendingAppointments(CancellationToken cancellationToken)
    {
        var query = new GetPendingAppointmentsQuery();
        var appointments = await _mediator.Send(query, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Get available time slots for a doctor on a specific date
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/available-slots")]
    public async Task<ActionResult<List<TimeSlotResponse>>> GetAvailableTimeSlots(
        Guid doctorId,
        [FromQuery] DateTime date,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableTimeSlotsQuery(doctorId, date);
        var timeSlots = await _mediator.Send(query, cancellationToken);
        return Ok(timeSlots);
    }
}

public record UpdateAppointmentRequest(
    DateTime AppointmentDateTime,
    int DurationMinutes,
    VoxMed.Domain.Entities.AppointmentType Type,
    string? Reason,
    string? Notes,
    string? Symptoms,
    string? Diagnosis,
    string? Treatment);

public record CancelAppointmentRequest(string? CancellationReason);
public record CompleteAppointmentRequest(string? Notes);
public record TimeSlotResponse(DateTime StartTime, DateTime EndTime, int DurationMinutes, bool IsAvailable);

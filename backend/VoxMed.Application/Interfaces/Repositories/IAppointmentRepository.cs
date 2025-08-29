using VoxMed.Domain.Entities;

namespace VoxMed.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing appointments
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Get appointment by ID
    /// </summary>
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all appointments
    /// </summary>
    Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new appointment
    /// </summary>
    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing appointment
    /// </summary>
    Task<Appointment> UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an appointment
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get appointments for a specific doctor
    /// </summary>
    Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(Guid doctorId);

    /// <summary>
    /// Get appointments for a specific patient
    /// </summary>
    Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(Guid patientId);

    /// <summary>
    /// Get appointments for a specific date range
    /// </summary>
    Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get appointments for a specific doctor and date range
    /// </summary>
    Task<IEnumerable<Appointment>> GetDoctorAppointmentsByDateRangeAsync(Guid doctorId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get upcoming appointments for a doctor
    /// </summary>
    Task<IEnumerable<Appointment>> GetUpcomingDoctorAppointmentsAsync(Guid doctorId);

    /// <summary>
    /// Get upcoming appointments for a patient
    /// </summary>
    Task<IEnumerable<Appointment>> GetUpcomingPatientAppointmentsAsync(Guid patientId);

    /// <summary>
    /// Check if a doctor has conflicting appointments for a given time slot
    /// </summary>
    Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime appointmentDateTime, int durationMinutes, Guid? excludeAppointmentId = null);

    /// <summary>
    /// Get appointments by status
    /// </summary>
    Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(AppointmentStatus status);

    /// <summary>
    /// Get today's appointments for a doctor
    /// </summary>
    Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync(Guid doctorId);

    /// <summary>
    /// Get pending appointments (awaiting confirmation)
    /// </summary>
    Task<IEnumerable<Appointment>> GetPendingAppointmentsAsync();

    /// <summary>
    /// Get available time slots for a doctor on a specific date
    /// </summary>
    Task<IEnumerable<TimeSlot>> GetAvailableTimeSlotsAsync(Guid doctorId, DateTime date);
}

/// <summary>
/// Represents an available time slot
/// </summary>
public class TimeSlot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsAvailable { get; set; }
}

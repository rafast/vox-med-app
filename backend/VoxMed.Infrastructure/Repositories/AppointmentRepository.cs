using Microsoft.EntityFrameworkCore;
using VoxMed.Application.Interfaces.Repositories;
using VoxMed.Domain.Entities;
using VoxMed.Infrastructure.Data;

namespace VoxMed.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return appointment;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return false;

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(Guid doctorId)
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(Guid patientId)
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.AppointmentDateTime >= startDate && a.AppointmentDateTime <= endDate)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetDoctorAppointmentsByDateRangeAsync(Guid doctorId, DateTime startDate, DateTime endDate)
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.DoctorId == doctorId 
                       && a.AppointmentDateTime >= startDate 
                       && a.AppointmentDateTime <= endDate)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingDoctorAppointmentsAsync(Guid doctorId)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.DoctorId == doctorId 
                       && a.AppointmentDateTime > now
                       && a.Status == AppointmentStatus.Scheduled)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingPatientAppointmentsAsync(Guid patientId)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.PatientId == patientId 
                       && a.AppointmentDateTime > now
                       && a.Status == AppointmentStatus.Scheduled)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime appointmentDateTime, int durationMinutes, Guid? excludeAppointmentId = null)
    {
        var appointmentEnd = appointmentDateTime.AddMinutes(durationMinutes);
        
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                       && a.Status != AppointmentStatus.Cancelled
                       && a.Status != AppointmentStatus.NoShow
                       && (excludeAppointmentId == null || a.Id != excludeAppointmentId)
                       && ((a.AppointmentDateTime <= appointmentDateTime && a.AppointmentDateTime.AddMinutes(a.DurationMinutes) > appointmentDateTime)
                           || (a.AppointmentDateTime < appointmentEnd && a.AppointmentDateTime >= appointmentDateTime)))
            .AnyAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(AppointmentStatus status)
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.Status == status)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync(Guid doctorId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.DoctorId == doctorId 
                       && a.AppointmentDateTime >= today 
                       && a.AppointmentDateTime < tomorrow)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetPendingAppointmentsAsync()
    {
        return await _context.Appointments
            .Include(a => a.Schedule)
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableTimeSlotsAsync(Guid doctorId, DateTime date)
    {
        // Get doctor's schedule for the day
        var dayOfWeek = date.DayOfWeek;

        var schedule = await _context.DoctorSchedules
            .FirstOrDefaultAsync(s => s.DoctorId == doctorId 
                                    && s.DayOfWeek == dayOfWeek
                                    && s.IsActive
                                    && (s.EffectiveFrom == null || DateOnly.FromDateTime(date) >= s.EffectiveFrom)
                                    && (s.EffectiveTo == null || DateOnly.FromDateTime(date) <= s.EffectiveTo));

        if (schedule == null)   
            return new List<TimeSlot>();

        // Get existing appointments for the day
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);
        
        var existingAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                       && a.AppointmentDateTime >= dayStart
                       && a.AppointmentDateTime < dayEnd
                       && a.Status != AppointmentStatus.Cancelled
                       && a.Status != AppointmentStatus.NoShow)
            .ToListAsync();

        // Generate time slots
        var timeSlots = new List<TimeSlot>();
        var currentTime = schedule.StartTime;
        
        while (currentTime < schedule.EndTime)
        {
            var slotStart = date.Date.Add(currentTime.ToTimeSpan());
            var slotEnd = slotStart.AddMinutes(schedule.SlotDurationMinutes);
            
            // Check if this slot conflicts with any existing appointment
            var hasConflict = existingAppointments.Any(a => 
                (a.AppointmentDateTime < slotEnd && a.AppointmentDateTime.AddMinutes(a.DurationMinutes) > slotStart));
            
            timeSlots.Add(new TimeSlot
            {
                StartTime = slotStart,
                EndTime = slotEnd,
                DurationMinutes = schedule.SlotDurationMinutes,
                IsAvailable = !hasConflict
            });
            
            currentTime = currentTime.AddMinutes(schedule.SlotDurationMinutes);
        }
        
        return timeSlots;
    }
}

using System.ComponentModel.DataAnnotations;

namespace VoxMed.Domain.Entities;

/// <summary>
/// Represents an available time slot for appointments
/// This is typically generated dynamically from DoctorSchedule and existing Appointments
/// </summary>
public class AvailableTimeSlot
{
    public Guid DoctorId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsAvailable { get; set; }

    public string? UnavailableReason { get; set; }

    // Computed properties
    public DateTime StartDateTime => Date.ToDateTime(StartTime);
    public DateTime EndDateTime => Date.ToDateTime(EndTime);
    public string TimeRange => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
}

/// <summary>
/// Helper class for schedule management operations
/// </summary>
public static class ScheduleHelper
{
    /// <summary>
    /// Generates available time slots for a doctor on a specific date
    /// </summary>
    public static List<AvailableTimeSlot> GenerateTimeSlots(
        DoctorSchedule schedule,
        DateOnly date,
        List<Appointment> existingAppointments,
        DoctorScheduleException? exception = null)
    {
        var slots = new List<AvailableTimeSlot>();

        // Determine working hours for the day
        TimeOnly startTime;
        TimeOnly endTime;
        int slotDuration;

        if (exception?.ExceptionType == ScheduleExceptionType.CustomHours)
        {
            if (!exception.StartTime.HasValue || !exception.EndTime.HasValue)
                return slots; // No custom hours defined

            startTime = exception.StartTime.Value;
            endTime = exception.EndTime.Value;
            slotDuration = exception.SlotDurationMinutes ?? schedule.SlotDurationMinutes;
        }
        else if (exception?.ExceptionType == ScheduleExceptionType.Unavailable ||
                 exception?.ExceptionType == ScheduleExceptionType.Vacation ||
                 exception?.ExceptionType == ScheduleExceptionType.Holiday)
        {
            return slots; // Doctor is not available
        }
        else
        {
            startTime = schedule.StartTime;
            endTime = schedule.EndTime;
            slotDuration = schedule.SlotDurationMinutes;
        }

        // Generate time slots
        var currentTime = startTime;
        while (currentTime.AddMinutes(slotDuration) <= endTime)
        {
            var slotEndTime = currentTime.AddMinutes(slotDuration);
            var slotStartDateTime = date.ToDateTime(currentTime);
            var slotEndDateTime = date.ToDateTime(slotEndTime);

            // Check if this slot conflicts with existing appointments
            bool isConflicted = existingAppointments.Any(apt =>
                apt.AppointmentDateTime < slotEndDateTime &&
                apt.AppointmentDateTime.AddMinutes(apt.DurationMinutes) > slotStartDateTime &&
                apt.Status != AppointmentStatus.Cancelled);

            slots.Add(new AvailableTimeSlot
            {
                DoctorId = schedule.DoctorId,
                Date = date,
                StartTime = currentTime,
                EndTime = slotEndTime,
                DurationMinutes = slotDuration,
                IsAvailable = !isConflicted,
                UnavailableReason = isConflicted ? "Already booked" : null
            });

            // Move to next slot (including break time)
            currentTime = currentTime.AddMinutes(slotDuration + schedule.BreakDurationMinutes);
        }

        return slots;
    }

    /// <summary>
    /// Checks if a doctor is available at a specific date and time
    /// </summary>
    public static bool IsAvailable(
        DoctorSchedule schedule,
        DateTime appointmentDateTime,
        int durationMinutes,
        List<Appointment> existingAppointments,
        DoctorScheduleException? exception = null)
    {
        var date = DateOnly.FromDateTime(appointmentDateTime);
        var time = TimeOnly.FromDateTime(appointmentDateTime);
        var endTime = time.AddMinutes(durationMinutes);

        // Check if it's within working hours
        if (exception?.ExceptionType == ScheduleExceptionType.CustomHours)
        {
            if (!exception.StartTime.HasValue || !exception.EndTime.HasValue)
                return false;

            if (time < exception.StartTime || endTime > exception.EndTime)
                return false;
        }
        else if (exception?.ExceptionType == ScheduleExceptionType.Unavailable ||
                 exception?.ExceptionType == ScheduleExceptionType.Vacation ||
                 exception?.ExceptionType == ScheduleExceptionType.Holiday)
        {
            return false;
        }
        else
        {
            if (time < schedule.StartTime || endTime > schedule.EndTime)
                return false;
        }

        // Check for conflicts with existing appointments
        var appointmentEndDateTime = appointmentDateTime.AddMinutes(durationMinutes);
        return !existingAppointments.Any(apt =>
            apt.AppointmentDateTime < appointmentEndDateTime &&
            apt.AppointmentDateTime.AddMinutes(apt.DurationMinutes) > appointmentDateTime &&
            apt.Status != AppointmentStatus.Cancelled);
    }
}

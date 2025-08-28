using VoxMed.Domain.Common;

namespace VoxMed.Domain.ValueObjects;

/// <summary>
/// Value object representing a time range (start time to end time)
/// </summary>
public class TimeRange : ValueObject
{
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public TimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Duration of this time range in minutes
    /// </summary>
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

    /// <summary>
    /// Checks if this time range overlaps with another
    /// </summary>
    public bool OverlapsWith(TimeRange other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    /// <summary>
    /// Checks if a specific time is within this range
    /// </summary>
    public bool Contains(TimeOnly time)
    {
        return time >= StartTime && time < EndTime;
    }

    /// <summary>
    /// Creates a new time range by extending this one
    /// </summary>
    public TimeRange ExtendBy(int minutes)
    {
        return new TimeRange(StartTime, EndTime.AddMinutes(minutes));
    }

    public override string ToString()
    {
        return $"{StartTime:HH:mm} - {EndTime:HH:mm}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }
}

/// <summary>
/// Value object representing an appointment duration with breaks
/// </summary>
public class AppointmentDuration : ValueObject
{
    public int AppointmentMinutes { get; private set; }
    public int BreakMinutes { get; private set; }

    public AppointmentDuration(int appointmentMinutes, int breakMinutes = 0)
    {
        if (appointmentMinutes < 5 || appointmentMinutes > 240)
            throw new ArgumentException("Appointment duration must be between 5 and 240 minutes");
        
        if (breakMinutes < 0 || breakMinutes > 60)
            throw new ArgumentException("Break duration must be between 0 and 60 minutes");

        AppointmentMinutes = appointmentMinutes;
        BreakMinutes = breakMinutes;
    }

    /// <summary>
    /// Total time including appointment and break
    /// </summary>
    public int TotalMinutes => AppointmentMinutes + BreakMinutes;

    public override string ToString()
    {
        return BreakMinutes > 0 
            ? $"{AppointmentMinutes}min + {BreakMinutes}min break"
            : $"{AppointmentMinutes}min";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AppointmentMinutes;
        yield return BreakMinutes;
    }
}

/// <summary>
/// Value object representing a date range
/// </summary>
public class DateRange : ValueObject
{
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public DateRange(DateOnly startDate, DateOnly? endDate = null)
    {
        if (endDate.HasValue && startDate > endDate.Value)
            throw new ArgumentException("Start date must be before or equal to end date");

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Creates an indefinite date range (no end date)
    /// </summary>
    public static DateRange StartingFrom(DateOnly startDate)
    {
        return new DateRange(startDate, null);
    }

    /// <summary>
    /// Creates a single-day date range
    /// </summary>
    public static DateRange SingleDay(DateOnly date)
    {
        return new DateRange(date, date);
    }

    /// <summary>
    /// Checks if a date falls within this range
    /// </summary>
    public bool Contains(DateOnly date)
    {
        return date >= StartDate && (EndDate == null || date <= EndDate.Value);
    }

    /// <summary>
    /// Checks if this range overlaps with another
    /// </summary>
    public bool OverlapsWith(DateRange other)
    {
        if (EndDate == null || other.EndDate == null)
        {
            // If either range is indefinite, check only start dates
            return StartDate <= (other.EndDate ?? DateOnly.MaxValue) &&
                   other.StartDate <= (EndDate ?? DateOnly.MaxValue);
        }

        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public override string ToString()
    {
        return EndDate.HasValue 
            ? $"{StartDate:yyyy-MM-dd} to {EndDate.Value:yyyy-MM-dd}"
            : $"From {StartDate:yyyy-MM-dd}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate ?? DateOnly.MaxValue;
    }
}

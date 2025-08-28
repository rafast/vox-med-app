using VoxMed.Domain.Common;

namespace VoxMed.Domain.Events;

// Doctor Schedule Events
/// <summary>
/// Domain event raised when a new doctor schedule is created
/// </summary>
public class DoctorScheduleCreatedEvent : DomainEvent
{
    public Guid ScheduleId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }

    public DoctorScheduleCreatedEvent(Guid scheduleId, Guid doctorId, DayOfWeek dayOfWeek)
    {
        ScheduleId = scheduleId;
        DoctorId = doctorId;
        DayOfWeek = dayOfWeek;
    }
}

/// <summary>
/// Domain event raised when a doctor schedule is updated
/// </summary>
public class DoctorScheduleUpdatedEvent : DomainEvent
{
    public Guid ScheduleId { get; private set; }
    public Guid DoctorId { get; private set; }

    public DoctorScheduleUpdatedEvent(Guid scheduleId, Guid doctorId)
    {
        ScheduleId = scheduleId;
        DoctorId = doctorId;
    }
}

/// <summary>
/// Domain event raised when a doctor schedule is activated
/// </summary>
public class DoctorScheduleActivatedEvent : DomainEvent
{
    public Guid ScheduleId { get; private set; }
    public Guid DoctorId { get; private set; }

    public DoctorScheduleActivatedEvent(Guid scheduleId, Guid doctorId)
    {
        ScheduleId = scheduleId;
        DoctorId = doctorId;
    }
}

/// <summary>
/// Domain event raised when a doctor schedule is deactivated
/// </summary>
public class DoctorScheduleDeactivatedEvent : DomainEvent
{
    public Guid ScheduleId { get; private set; }
    public Guid DoctorId { get; private set; }

    public DoctorScheduleDeactivatedEvent(Guid scheduleId, Guid doctorId)
    {
        ScheduleId = scheduleId;
        DoctorId = doctorId;
    }
}

// Appointment Events
/// <summary>
/// Domain event raised when a new appointment is created
/// </summary>
public class AppointmentCreatedEvent : DomainEvent
{
    public Guid AppointmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateTime AppointmentDateTime { get; private set; }

    public AppointmentCreatedEvent(Guid appointmentId, Guid doctorId, Guid patientId, DateTime appointmentDateTime)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
        AppointmentDateTime = appointmentDateTime;
    }
}

/// <summary>
/// Domain event raised when an appointment is confirmed
/// </summary>
public class AppointmentConfirmedEvent : DomainEvent
{
    public Guid AppointmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }

    public AppointmentConfirmedEvent(Guid appointmentId, Guid doctorId, Guid patientId)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
    }
}

/// <summary>
/// Domain event raised when an appointment is cancelled
/// </summary>
public class AppointmentCancelledEvent : DomainEvent
{
    public Guid AppointmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }
    public string Reason { get; private set; }

    public AppointmentCancelledEvent(Guid appointmentId, Guid doctorId, Guid patientId, string reason)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
        Reason = reason;
    }
}

/// <summary>
/// Domain event raised when an appointment is started
/// </summary>
public class AppointmentStartedEvent : DomainEvent
{
    public Guid AppointmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }

    public AppointmentStartedEvent(Guid appointmentId, Guid doctorId, Guid patientId)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
    }
}

/// <summary>
/// Domain event raised when an appointment is completed
/// </summary>
public class AppointmentCompletedEvent : DomainEvent
{
    public Guid AppointmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }

    public AppointmentCompletedEvent(Guid appointmentId, Guid doctorId, Guid patientId)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
    }
}

/// <summary>
/// Domain event raised when a patient doesn't show up for an appointment
/// </summary>
public class AppointmentNoShowEvent : DomainEvent
{
    public Guid AppointmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }

    public AppointmentNoShowEvent(Guid appointmentId, Guid doctorId, Guid patientId)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
    }
}

/// <summary>
/// Domain event raised when an appointment is rescheduled
/// </summary>
public class AppointmentRescheduledEvent : DomainEvent
{
    public Guid AppointmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateTime OldDateTime { get; private set; }
    public DateTime NewDateTime { get; private set; }

    public AppointmentRescheduledEvent(Guid appointmentId, Guid doctorId, Guid patientId, DateTime oldDateTime, DateTime newDateTime)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        PatientId = patientId;
        OldDateTime = oldDateTime;
        NewDateTime = newDateTime;
    }
}

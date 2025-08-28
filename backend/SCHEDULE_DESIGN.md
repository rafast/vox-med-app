# Doctor Schedule Database Design

## Overview

This document outlines the database design for managing doctor schedules in the VoxMed application. The design is flexible, scalable, and handles complex scheduling scenarios including regular schedules, exceptions, and appointments.

## Database Schema

### 1. Core Tables

#### DoctorSchedules
Stores the regular weekly schedule for each doctor.

```sql
DoctorSchedules {
  Id: UUID (PK)
  DoctorId: UUID (FK to ApplicationUsers)
  DayOfWeek: int (0=Sunday, 1=Monday, ..., 6=Saturday)
  StartTime: TimeOnly
  EndTime: TimeOnly
  SlotDurationMinutes: int (default: 30)
  BreakDurationMinutes: int (default: 5)
  IsActive: boolean (default: true)
  EffectiveFrom: DateOnly? (optional)
  EffectiveTo: DateOnly? (optional)
  CreatedAt: DateTime
  UpdatedAt: DateTime
}
```

#### DoctorScheduleExceptions
Handles exceptions to the regular schedule (holidays, custom hours, etc.).

```sql
DoctorScheduleExceptions {
  Id: UUID (PK)
  DoctorId: UUID (FK to ApplicationUsers)
  Date: DateOnly
  ExceptionType: ScheduleExceptionType
  StartTime: TimeOnly? (for custom hours)
  EndTime: TimeOnly? (for custom hours)
  SlotDurationMinutes: int? (for custom slots)
  Reason: string(500)?
  IsRecurring: boolean (default: false)
  CreatedAt: DateTime
  UpdatedAt: DateTime
}
```

#### Appointments
Stores actual appointments between doctors and patients.

```sql
Appointments {
  Id: UUID (PK)
  DoctorId: UUID (FK to ApplicationUsers)
  PatientId: UUID (FK to ApplicationUsers)
  ScheduleId: UUID? (FK to DoctorSchedules)
  AppointmentDateTime: DateTime
  DurationMinutes: int (default: 30)
  Status: AppointmentStatus
  Type: AppointmentType
  Reason: string(1000)?
  Notes: string(2000)?
  Symptoms: string(1000)?
  Diagnosis: string(1000)?
  Treatment: string(2000)?
  IsOnline: boolean (default: false)
  Location: string(200)?
  CreatedAt: DateTime
  UpdatedAt: DateTime
  ConfirmedAt: DateTime?
  CancelledAt: DateTime?
  CancellationReason: string(500)?
}
```

## Design Principles

### 1. **Flexibility**
- Supports different working hours for each day
- Allows custom slot durations per doctor/schedule
- Handles exceptions and special cases
- Supports both recurring and one-time schedule changes

### 2. **Performance**
- Strategic indexing for common queries
- Efficient time slot generation algorithms
- Optimized for appointment booking scenarios

### 3. **Data Integrity**
- Check constraints ensure valid time ranges
- Unique constraints prevent double-booking
- Foreign key relationships maintain referential integrity
- Proper validation at database level

### 4. **Scalability**
- Designed to handle multiple doctors and patients
- Efficient querying for large datasets
- Support for future features (recurring appointments, etc.)

## Key Features

### 1. **Regular Schedules**
```csharp
// Example: Doctor works Monday-Friday 9AM-5PM with 30-minute slots
var schedule = new DoctorSchedule {
    DoctorId = doctorId,
    DayOfWeek = DayOfWeek.Monday,
    StartTime = new TimeOnly(9, 0),
    EndTime = new TimeOnly(17, 0),
    SlotDurationMinutes = 30,
    BreakDurationMinutes = 5
};
```

### 2. **Schedule Exceptions**
```csharp
// Example: Doctor is unavailable on Christmas
var exception = new DoctorScheduleException {
    DoctorId = doctorId,
    Date = new DateOnly(2024, 12, 25),
    ExceptionType = ScheduleExceptionType.Holiday,
    Reason = "Christmas Day"
};

// Example: Doctor has custom hours on Saturday
var customHours = new DoctorScheduleException {
    DoctorId = doctorId,
    Date = new DateOnly(2024, 12, 14),
    ExceptionType = ScheduleExceptionType.CustomHours,
    StartTime = new TimeOnly(10, 0),
    EndTime = new TimeOnly(14, 0),
    SlotDurationMinutes = 60
};
```

### 3. **Appointment Booking**
```csharp
// Example: Book an appointment
var appointment = new Appointment {
    DoctorId = doctorId,
    PatientId = patientId,
    AppointmentDateTime = new DateTime(2024, 12, 15, 10, 30, 0),
    DurationMinutes = 30,
    Type = AppointmentType.Consultation,
    Reason = "Regular checkup"
};
```

## Common Queries

### 1. **Get Doctor's Schedule for a Week**
```csharp
var schedules = await context.DoctorSchedules
    .Where(s => s.DoctorId == doctorId && s.IsActive)
    .Include(s => s.Exceptions.Where(e => e.Date >= startDate && e.Date <= endDate))
    .ToListAsync();
```

### 2. **Get Available Time Slots**
```csharp
var availableSlots = ScheduleHelper.GenerateTimeSlots(
    schedule: doctorSchedule,
    date: requestedDate,
    existingAppointments: appointments,
    exception: scheduleException
);
```

### 3. **Check Availability**
```csharp
bool isAvailable = ScheduleHelper.IsAvailable(
    schedule: doctorSchedule,
    appointmentDateTime: requestedDateTime,
    durationMinutes: 30,
    existingAppointments: appointments,
    exception: scheduleException
);
```

## Enums

### ScheduleExceptionType
- `Unavailable` - Doctor not available
- `CustomHours` - Different working hours
- `Vacation` - On vacation
- `Conference` - Attending conference/training
- `Emergency` - Emergency/sick leave
- `Holiday` - Public holiday

### AppointmentStatus
- `Scheduled` - Confirmed appointment
- `Pending` - Waiting for confirmation
- `Cancelled` - Cancelled appointment
- `Completed` - Finished appointment
- `NoShow` - Patient didn't show up
- `InProgress` - Currently happening
- `Rescheduled` - Needs rescheduling

### AppointmentType
- `Consultation` - Regular consultation
- `FollowUp` - Follow-up appointment
- `Emergency` - Emergency appointment
- `CheckUp` - Routine check-up
- `Vaccination` - Vaccination
- `Diagnostic` - Diagnostic procedure
- `Procedure` - Surgery/procedure
- `Telemedicine` - Online consultation

## Performance Considerations

### Indexes
- `IX_DoctorSchedules_Doctor_Day_Active` - Fast schedule lookups
- `IX_Appointments_Doctor_DateTime` - Efficient appointment queries
- `IX_DoctorScheduleExceptions_Doctor_Date` - Quick exception checks

### Constraints
- Unique constraint on doctor-datetime-status prevents double-booking
- Check constraints ensure data validity
- Time range validations at database level

## Future Enhancements

1. **Recurring Appointments** - Support for regular recurring appointments
2. **Resource Management** - Track rooms, equipment, etc.
3. **Waiting Lists** - Manage patient waiting lists for cancelled slots
4. **Time Zone Support** - Handle multiple time zones
5. **Integration APIs** - Connect with external calendar systems
6. **Analytics** - Doctor utilization and patient flow analytics

This design provides a solid foundation for a comprehensive medical appointment scheduling system while maintaining flexibility for future requirements.

using Microsoft.EntityFrameworkCore;
using VoxMed.Domain.Entities;

namespace VoxMed.Infrastructure.Data;

/// <summary>
/// Main application database context for VoxMed domain entities
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Schedule-related entities
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
    public DbSet<DoctorScheduleException> DoctorScheduleExceptions { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureDoctorSchedule(modelBuilder);
        ConfigureDoctorScheduleException(modelBuilder);
        ConfigureAppointment(modelBuilder);
    }

    private void ConfigureDoctorSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.ToTable("DoctorSchedules", t =>
            {
                t.HasCheckConstraint("CK_DoctorSchedules_TimeRange", 
                    "\"StartTime\" < \"EndTime\"");
                t.HasCheckConstraint("CK_DoctorSchedules_SlotDuration", 
                    "\"SlotDurationMinutes\" BETWEEN 5 AND 240");
                t.HasCheckConstraint("CK_DoctorSchedules_BreakDuration", 
                    "\"BreakDurationMinutes\" BETWEEN 0 AND 60");
            });
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.DoctorId)
                .IsRequired();

            entity.Property(e => e.DayOfWeek)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.StartTime)
                .IsRequired();

            entity.Property(e => e.EndTime)
                .IsRequired();

            entity.Property(e => e.SlotDurationMinutes)
                .IsRequired()
                .HasDefaultValue(30);

            entity.Property(e => e.BreakDurationMinutes)
                .HasDefaultValue(5);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.DateAdded)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.DateUpdated)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes for performance
            entity.HasIndex(e => e.DoctorId)
                .HasDatabaseName("IX_DoctorSchedules_DoctorId");

            entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek, e.IsActive })
                .HasDatabaseName("IX_DoctorSchedules_Doctor_Day_Active");

            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo })
                .HasDatabaseName("IX_DoctorSchedules_EffectiveDates");
        });
    }

    private void ConfigureDoctorScheduleException(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DoctorScheduleException>(entity =>
        {
            entity.ToTable("DoctorScheduleExceptions", t =>
            {
                t.HasCheckConstraint("CK_DoctorScheduleExceptions_CustomHours", 
                    "(\"ExceptionType\" != 2) OR (\"StartTime\" IS NOT NULL AND \"EndTime\" IS NOT NULL AND \"StartTime\" < \"EndTime\")");
            });
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.DoctorId)
                .IsRequired();

            entity.Property(e => e.Date)
                .IsRequired();

            entity.Property(e => e.ExceptionType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Reason)
                .HasMaxLength(500);

            entity.Property(e => e.IsRecurring)
                .HasDefaultValue(false);

            entity.Property(e => e.DateAdded)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.DateUpdated)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            entity.HasIndex(e => e.DoctorId)
                .HasDatabaseName("IX_DoctorScheduleExceptions_DoctorId");

            entity.HasIndex(e => new { e.DoctorId, e.Date })
                .IsUnique()
                .HasDatabaseName("IX_DoctorScheduleExceptions_Doctor_Date");

            entity.HasIndex(e => e.Date)
                .HasDatabaseName("IX_DoctorScheduleExceptions_Date");
        });
    }

    private void ConfigureAppointment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments", t =>
            {
                t.HasCheckConstraint("CK_Appointments_Duration", 
                    "\"DurationMinutes\" BETWEEN 5 AND 240");
                t.HasCheckConstraint("CK_Appointments_FutureDate", 
                    "\"AppointmentDateTime\" > CURRENT_TIMESTAMP - INTERVAL '1 day'");
            });
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.DoctorId)
                .IsRequired();

            entity.Property(e => e.PatientId)
                .IsRequired();

            entity.Property(e => e.AppointmentDateTime)
                .IsRequired();

            entity.Property(e => e.DurationMinutes)
                .IsRequired()
                .HasDefaultValue(30);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(AppointmentStatus.Scheduled);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(AppointmentType.Consultation);

            entity.Property(e => e.Reason)
                .HasMaxLength(1000);

            entity.Property(e => e.Notes)
                .HasMaxLength(2000);

            entity.Property(e => e.Symptoms)
                .HasMaxLength(1000);

            entity.Property(e => e.Diagnosis)
                .HasMaxLength(1000);

            entity.Property(e => e.Treatment)
                .HasMaxLength(2000);

            entity.Property(e => e.Location)
                .HasMaxLength(200);

            entity.Property(e => e.CancellationReason)
                .HasMaxLength(500);

            entity.Property(e => e.IsOnline)
                .HasDefaultValue(false);

            entity.Property(e => e.DateAdded)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.DateUpdated)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.Schedule)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            entity.HasIndex(e => e.DoctorId)
                .HasDatabaseName("IX_Appointments_DoctorId");

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("IX_Appointments_PatientId");

            entity.HasIndex(e => e.AppointmentDateTime)
                .HasDatabaseName("IX_Appointments_DateTime");

            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDateTime })
                .HasDatabaseName("IX_Appointments_Doctor_DateTime");

            entity.HasIndex(e => new { e.PatientId, e.AppointmentDateTime })
                .HasDatabaseName("IX_Appointments_Patient_DateTime");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Appointments_Status");

            // Unique constraint to prevent double-booking
            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDateTime, e.Status })
                .IsUnique()
                .HasDatabaseName("IX_Appointments_Doctor_DateTime_Status")
                .HasFilter("\"Status\" IN (1, 6)"); // Scheduled or InProgress
        });
    }
}

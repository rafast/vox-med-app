using Microsoft.EntityFrameworkCore;
using VoxMedApi.Models;

namespace VoxMedApi.Data;

public class VoxMedDbContext : DbContext
{
    public VoxMedDbContext(DbContextOptions<VoxMedDbContext> options) : base(options)
    {
    }

    // User-related entities
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    // Medical entities
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }

    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schemas
        modelBuilder.HasDefaultSchema("public");

        // Configure User entities
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", "users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Permissions).HasColumnType("jsonb");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            
            // Relationships
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId);
        });

        // Configure Medical entities
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients", "medical");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.PatientNumber).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Address).HasColumnType("jsonb");
            entity.Property(e => e.EmergencyContact).HasColumnType("jsonb");
            entity.Property(e => e.MedicalHistory).HasColumnType("jsonb");
            entity.Property(e => e.CurrentMedications).HasColumnType("jsonb");
            entity.Property(e => e.Allergies).HasColumnType("text[]");

            // Relationships
            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments", "medical");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("scheduled");
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);

            // Relationships
            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(e => e.PatientId);
            
            entity.HasOne(e => e.Doctor)
                  .WithMany()
                  .HasForeignKey(e => e.DoctorId);
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.ToTable("medical_records", "medical");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecordType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Diagnosis).HasColumnType("jsonb");
            entity.Property(e => e.TreatmentPlan).HasColumnType("jsonb");
            entity.Property(e => e.Medications).HasColumnType("jsonb");
            entity.Property(e => e.Attachments).HasColumnType("jsonb");
            entity.Property(e => e.Symptoms).HasColumnType("text[]");

            // Relationships
            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.MedicalRecords)
                  .HasForeignKey(e => e.PatientId);
            
            entity.HasOne(e => e.Doctor)
                  .WithMany()
                  .HasForeignKey(e => e.DoctorId);
            
            entity.HasOne(e => e.Appointment)
                  .WithMany()
                  .HasForeignKey(e => e.AppointmentId)
                  .IsRequired(false);
        });

        // Configure Audit
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_log", "audit");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Operation).IsRequired().HasMaxLength(10);
            entity.Property(e => e.OldData).HasColumnType("jsonb");
            entity.Property(e => e.NewData).HasColumnType("jsonb");
        });

        // Configure automatic timestamps
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.Name == "CreatedAt" || property.Name == "UpdatedAt")
                {
                    property.SetColumnType("timestamp with time zone");
                    if (property.Name == "CreatedAt")
                    {
                        property.SetDefaultValueSql("NOW()");
                    }
                }
                if (property.Name == "LastLogin")
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }
    }
}

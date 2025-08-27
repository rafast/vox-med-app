using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VoxMed.Auth.Models;

namespace VoxMed.Auth.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize table names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MedicalLicenseNumber).HasMaxLength(50);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.PatientId).HasMaxLength(50);
            entity.Property(e => e.BloodType).HasMaxLength(10);
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.MedicalLicenseNumber).IsUnique();
            entity.HasIndex(e => e.PatientId).IsUnique();
        });

        // Configure ApplicationRole
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(250);
        });

        // Seed roles
        var doctorRoleId = Guid.NewGuid();
        var patientRoleId = Guid.NewGuid();

        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = doctorRoleId,
                Name = "Doctor",
                NormalizedName = "DOCTOR",
                Description = "Medical doctors and physicians",
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationRole
            {
                Id = patientRoleId,
                Name = "Patient",
                NormalizedName = "PATIENT",
                Description = "Patients receiving medical care",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}

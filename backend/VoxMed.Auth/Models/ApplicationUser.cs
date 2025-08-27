using Microsoft.AspNetCore.Identity;

namespace VoxMed.Auth.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public UserType UserType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Doctor-specific properties
    public string? MedicalLicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Department { get; set; }

    // Patient-specific properties
    public string? PatientId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public string? EmergencyContact { get; set; }

    public string UserTypeString => UserType.ToString();
}

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
        CreatedAt = DateTime.UtcNow;
    }
}

public enum UserType
{
    Doctor = 1,
    Patient = 2
}

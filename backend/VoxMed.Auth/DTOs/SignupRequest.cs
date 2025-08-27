using System.ComponentModel.DataAnnotations;
using VoxMed.Auth.Models;

namespace VoxMed.Auth.DTOs;

public class SignupRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public UserType UserType { get; set; }

    // Doctor-specific fields
    public string? MedicalLicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Department { get; set; }

    // Patient-specific fields
    public string? PatientId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Phone { get; set; }
}

using System.ComponentModel.DataAnnotations;
using VoxMed.Auth.Models;

namespace VoxMed.Auth.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public string UserTypeString { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Doctor-specific properties
    public string? MedicalLicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Department { get; set; }
    
    // Patient-specific properties
    public string? PatientId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public string? EmergencyContact { get; set; }
    
    public string? Phone { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoginResponse? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

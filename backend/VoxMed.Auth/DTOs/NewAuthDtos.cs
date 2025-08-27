using System.ComponentModel.DataAnnotations;
using VoxMed.Auth.Models;

namespace VoxMed.Auth.DTOs;

public record LoginRequestDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record LoginResponseDto(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoDto User
);

public record UserInfoDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserType UserType,
    string UserTypeString,
    bool IsActive,
    string? MedicalLicenseNumber,
    string? Specialization,
    string? Department,
    string? PatientId,
    DateTime? DateOfBirth,
    string? BloodType,
    string? EmergencyContact,
    string? Phone,
    DateTime? LastLoginAt
);

public record AuthResponseDto(
    bool Success,
    string Message,
    LoginResponseDto? Data = null,
    List<string>? Errors = null
);

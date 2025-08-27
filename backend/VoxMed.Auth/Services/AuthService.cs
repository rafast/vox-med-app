using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VoxMed.Auth.Models;
using VoxMed.Auth.DTOs;
using VoxMed.Auth.Interfaces;

namespace VoxMed.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<VoxMed.Auth.DTOs.AuthResponse> LoginAsync(VoxMed.Auth.DTOs.LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = { "Invalid credentials" }
                };
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {UserId} is not active", user.Id);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Account is deactivated",
                    Errors = { "Account deactivated" }
                };
            }

            // Verify password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed for user {UserId}: {Reason}", user.Id, result.ToString());
                
                if (result.IsLockedOut)
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Account locked due to multiple failed attempts",
                        Errors = { "Account locked" }
                    };
                
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = { "Invalid credentials" }
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var token = await _jwtTokenService.GenerateJwtTokenAsync(user, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Create response
            var loginResponse = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = MapToUserDto(user)
            };

            _logger.LogInformation("Login successful for user {UserId} ({UserType})", user.Id, user.UserType);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Data = loginResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during login",
                Errors = { "Internal server error" }
            };
        }
    }

    public async Task<AuthResponse> SignupAsync(VoxMed.Auth.DTOs.SignupRequest request)
    {
        try
        {
            _logger.LogInformation("Signup attempt for email {Email} as {UserType}", request.Email, request.UserType);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User already exists",
                    Errors = { "A user with this email already exists" }
                };
            }

            // Create new user
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserType = request.UserType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true // Auto-confirm for demo purposes
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Failed to create user",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Add user to role based on UserType
            string roleName = request.UserType.ToString();
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }

            _logger.LogInformation("User created successfully {UserId} ({UserType})", user.Id, user.UserType);

            return new AuthResponse
            {
                Success = true,
                Message = "User created successfully",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup for email {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during signup",
                Errors = { "Internal server error" }
            };
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id {UserId}", userId);
            return null;
        }
    }

    private static UserDto MapToUserDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            UserType = user.UserType,
            UserTypeString = user.UserTypeString,
            IsActive = user.IsActive,
            MedicalLicenseNumber = user.MedicalLicenseNumber,
            Specialization = user.Specialization,
            Department = user.Department,
            PatientId = user.PatientId,
            DateOfBirth = user.DateOfBirth,
            BloodType = user.BloodType,
            EmergencyContact = user.EmergencyContact,
            Phone = user.PhoneNumber,
            LastLoginAt = user.LastLoginAt
        };
    }

    public Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // For now, return a not implemented response
            // In a full implementation, you would validate the refresh token
            // and generate a new JWT token
            return Task.FromResult(new AuthResponse
            {
                Success = false,
                Message = "Refresh token functionality not implemented yet",
                Errors = { "Not implemented" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Task.FromResult(new AuthResponse
            {
                Success = false,
                Message = "An error occurred during token refresh",
                Errors = { "Internal server error" }
            });
        }
    }

    public async Task LogoutAsync(string userEmail)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                // For now, just log the logout
                // In a full implementation, you might invalidate refresh tokens
                _logger.LogInformation("User {UserId} logged out", user.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for email {Email}", userEmail);
        }
    }

    public async Task<UserDto?> GetUserInfoAsync(string userEmail)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for email {Email}", userEmail);
            return null;
        }
    }

    public async Task<AuthResponse> CreateDoctorUserAsync()
    {
        try
        {
            // Check if doctor user already exists
            var existingUser = await _userManager.FindByEmailAsync("doctor@voxmed.com");
            if (existingUser != null)
            {
                // Delete existing doctor user first
                await _userManager.DeleteAsync(existingUser);
            }

            // Create new doctor user
            var doctorUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "doctor@voxmed.com",
                Email = "doctor@voxmed.com",
                FirstName = "Dr. John",
                LastName = "Smith",
                UserType = UserType.Doctor,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                MedicalLicenseNumber = "DOC001",
                Specialization = "Cardiology",
                Department = "Cardiology Department"
            };

            var result = await _userManager.CreateAsync(doctorUser, "Doctor123!");
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Doctor user created successfully with email: {Email}", doctorUser.Email);
                return new AuthResponse
                {
                    Success = true,
                    Message = "Doctor user created successfully. Email: doctor@voxmed.com, Password: Doctor123!",
                    Data = null
                };
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create doctor user: {Errors}", errors);
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Failed to create doctor user: {errors}",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor user");
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred while creating doctor user",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResponse> CreateTestUserAsync()
    {
        try
        {
            // Check if test user already exists
            var existingUser = await _userManager.FindByEmailAsync("admin@voxmed.com");
            if (existingUser != null)
            {
                // Delete existing test user first
                await _userManager.DeleteAsync(existingUser);
            }

            // Create new test user
            var testUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin@voxmed.com",
                Email = "admin@voxmed.com",
                FirstName = "Admin",
                LastName = "User",
                UserType = UserType.Doctor,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                MedicalLicenseNumber = "TEST001",
                Specialization = "General Medicine",
                Department = "Internal Medicine"
            };

            var result = await _userManager.CreateAsync(testUser, "Admin123!");
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Test user created successfully with email: {Email}", testUser.Email);
                return new AuthResponse
                {
                    Success = true,
                    Message = "Test user created successfully",
                    Data = null
                };
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create test user: {Errors}", errors);
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Failed to create test user: {errors}",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test user");
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred while creating test user",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}

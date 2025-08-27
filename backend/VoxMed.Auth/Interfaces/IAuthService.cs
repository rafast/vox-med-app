namespace VoxMed.Auth.Interfaces;

public interface IAuthService
{
    Task<DTOs.AuthResponse> LoginAsync(DTOs.LoginRequest request);
    Task<DTOs.AuthResponse> SignupAsync(DTOs.SignupRequest request);
    Task<DTOs.UserDto?> GetUserByIdAsync(Guid userId);
    Task<DTOs.AuthResponse> CreateTestUserAsync();
    Task<DTOs.AuthResponse> CreateDoctorUserAsync();
}

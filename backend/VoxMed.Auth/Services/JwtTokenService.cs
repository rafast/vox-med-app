using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VoxMed.Auth.Models;

namespace VoxMed.Auth.Services;

public interface IJwtTokenService
{
    Task<string> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> GenerateJwtTokenAsync(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");
        var expirationHours = int.Parse(jwtSettings["ExpirationHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("user_type", user.UserType.ToString()),
            new("is_active", user.IsActive.ToString()),
            new("full_name", user.FullName),
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add user type specific claims
        if (user.UserType == UserType.Doctor)
        {
            if (!string.IsNullOrEmpty(user.MedicalLicenseNumber))
                claims.Add(new Claim("medical_license", user.MedicalLicenseNumber));
            
            if (!string.IsNullOrEmpty(user.Specialization))
                claims.Add(new Claim("specialization", user.Specialization));
            
            if (!string.IsNullOrEmpty(user.Department))
                claims.Add(new Claim("department", user.Department));
        }
        else if (user.UserType == UserType.Patient)
        {
            if (!string.IsNullOrEmpty(user.PatientId))
                claims.Add(new Claim("patient_id", user.PatientId));
            
            if (user.DateOfBirth.HasValue)
                claims.Add(new Claim("date_of_birth", user.DateOfBirth.Value.ToString("yyyy-MM-dd")));
            
            if (!string.IsNullOrEmpty(user.BloodType))
                claims.Add(new Claim("blood_type", user.BloodType));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}

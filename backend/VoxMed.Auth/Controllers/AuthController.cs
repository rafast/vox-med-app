using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VoxMed.Auth.DTOs;
using VoxMed.Auth.Interfaces;

namespace VoxMed.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Signup endpoint for creating new users (doctors and patients)
    /// </summary>
    /// <param name="request">Signup request containing user information</param>
    /// <returns>Authentication response with success/error information</returns>
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Signup([FromBody] SignupRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.SignupAsync(request);

            if (!result.Success)
            {
                _logger.LogWarning("Signup failed for email: {Email}. Reason: {Message}", 
                    request.Email, result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("User signed up successfully: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup attempt for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during signup" });
        }
    }

    /// <summary>
    /// Login endpoint for doctors and patients
    /// </summary>
    /// <param name="request">Login request containing email and PIN</param>
    /// <returns>Authentication response with JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                _logger.LogWarning("Login failed for email: {Email}. Reason: {Message}", 
                    request.Email, result.Message);
                return Unauthorized(new { message = result.Message });
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }


    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult GetHealth()
    {
        return Ok(new { 
            status = "Healthy", 
            service = "VoxMed Authentication API",
            timestamp = DateTime.UtcNow 
        });
    }

    /// <summary>
    /// TEMPORARY: Create doctor user endpoint (for development only)
    /// </summary>
    [HttpPost("create-doctor")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateDoctor()
    {
        try
        {
            var result = await _authService.CreateDoctorUserAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor user");
            return StatusCode(500, new { message = "An error occurred creating doctor user" });
        }
    }

    /// <summary>
    /// TEMPORARY: Create test user endpoint (for development only)
    /// </summary>
    [HttpPost("create-test-user")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateTestUser()
    {
        try
        {
            var result = await _authService.CreateTestUserAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test user");
            return StatusCode(500, new { message = "An error occurred creating test user" });
        }
    }
}

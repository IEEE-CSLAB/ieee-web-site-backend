using IEEEBackend.Dtos;
using IEEEBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAdminRepository _adminRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAdminRepository adminRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _adminRepository = adminRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    /// <summary>
    /// Health check endpoint for monitoring and keeping the server alive
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "IEEE Backend API"
        });
    }

    /// <summary>
    /// Admin login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var admin = await _adminRepository.GetByUsernameAsync(loginDto.Username);
        if (admin == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        if (!_passwordHasher.VerifyPassword(loginDto.Password, admin.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var token = _jwtService.GenerateToken(admin);
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        var response = new LoginResponseDto
        {
            Token = token,
            Username = admin.Username,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return Ok(response);
    }

    /// <summary>
    /// Change admin password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current admin ID from JWT token
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out var adminId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        // Get admin from database
        var admin = await _adminRepository.GetByIdAsync(adminId);
        if (admin == null)
        {
            return NotFound(new { message = "Admin not found." });
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, admin.PasswordHash))
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }

        // Hash new password
        var newPasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);

        // Update password
        var updated = await _adminRepository.UpdatePasswordAsync(adminId, newPasswordHash);
        if (!updated)
        {
            return StatusCode(500, new { message = "Failed to update password." });
        }

        return Ok(new { message = "Password changed successfully." });
    }
}


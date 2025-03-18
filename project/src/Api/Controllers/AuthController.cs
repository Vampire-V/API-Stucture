using Api.Extensions;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserWorkflow _userWorkflow;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserWorkflow userWorkflow,
        ILogger<AuthController> logger,
        IHostEnvironment hostEnvironment
    )
    {
        _userWorkflow = userWorkflow;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _hostEnvironment = hostEnvironment;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // สำหรับการลงทะเบียนสำเร็จ
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)] // ข้อมูลไม่ถูกต้อง
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)] // ความขัดแย้ง เช่น Email ซ้ำ
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration request.");
            return BadRequest(ModelState);
        }

        var result = await _userWorkflow.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Registration failed: {Message}", result.Message);
            return Conflict(new { Message = result.Message });
        }

        return Ok(new { Message = result.Message });
    }

    /// <summary>
    /// Authenticates a user and generates a token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)] // ล็อกอินสำเร็จ
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)] // ข้อมูลไม่ถูกต้อง
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)] // ล็อกอินไม่สำเร็จ
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login request.");
            return BadRequest(ModelState);
        }

        var result = await _userWorkflow.LoginAsync(request);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Login failed: {Message}", result.Message);
            return Unauthorized(new { Message = result.Message });
        }
        // ส่ง Refresh Token ใน Cookie
        // var refreshToken = result.Data?.RefreshToken ?? string.Empty;
        // Response.Cookies.Append("RefreshToken", refreshToken, CreateCookieOptions());

        return Ok(result.Data);
    }

    /// <summary>
    /// Refreshes a user's token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)] // Refresh Token สำเร็จ
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)] // ข้อมูลไม่ถูกต้อง
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)] // Refresh Token ไม่สำเร็จ
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid refresh token request.");
            return BadRequest(ModelState);
        }

        var result = await _userWorkflow.RefreshTokenAsync(request.RefreshToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Refresh token failed: {Message}", result.Message);
            return Unauthorized(new { Message = result.Message });
        }

        // อัปเดต Refresh Token ใหม่ใน Cookie
        // Response.Cookies.Append(
        //     "RefreshToken",
        //     result.Data?.RefreshToken ?? string.Empty,
        //     CreateCookieOptions()
        // );

        return Ok(result.Data);
    }

    /// <summary>
    /// Validate a JWT Token
    /// </summary>
    /// <returns>Validation result</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)] // UserInfo สำเร็จ
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)] // UserInfo ไม่สำเร็จ
    public async Task<IActionResult> GetMe()
    {
        // ดึง UserId จาก Claims
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { Message = "Invalid user claims." });
        }

        var result = await _userWorkflow.UserInfoAsync(userId);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { Message = result.Message });
        }
        return Ok(result.Data);
    }

    private CookieOptions CreateCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = false,
            Secure = _hostEnvironment.IsProduction(),
            SameSite = _hostEnvironment.IsProduction() ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };
    }
}

using System.Security.Cryptography;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Workflows;

public class UserWorkflow : IUserWorkflow
{
    private readonly ILogger<UserWorkflow> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public UserWorkflow(
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        ILogger<UserWorkflow> logger
    )
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Handles user registration
    /// </summary>
    public async Task<Result<object>> RegisterAsync(RegisterRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // ตรวจสอบความซ้ำซ้อนของ Email
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                await _unitOfWork.RollbackAsync();
                return Result<object>.Failure("Email already exists.");
            }

            // เตรียมข้อมูลในหน่วยความจำ
            var hashedPassword = _passwordHasher.HashPassword(request.Password);
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hashedPassword,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            };

            // เพิ่มผู้ใช้ใหม่
            await _unitOfWork.Users.AddAsync(newUser);

            // บันทึกการเปลี่ยนแปลง
            var commitResult = await _unitOfWork.CommitAsync();
            if (!commitResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return Result<object>.Failure($"Commit failed: {commitResult.Message}");
            }

            return Result<object>.Success("User registered successfully.");
        }
        catch (Exception ex)
        {
            // จัดการข้อผิดพลาด
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "An error occurred during user registration.");
            return Result<object>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles user login
    /// </summary>
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ตรวจสอบว่าผู้ใช้มีอยู่หรือไม่
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid email or password.");
            }

            // ตรวจสอบรหัสผ่าน
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result<LoginResponse>.Failure("Invalid email or password.");
            }

            // ตรวจสอบสถานะผู้ใช้งาน (ถ้ามี)
            if (user.IsLockedOut) // ตัวอย่าง: ตรวจสอบว่าผู้ใช้ถูกล็อกหรือไม่
            {
                return Result<LoginResponse>.Failure(
                    "Your account is locked. Please contact support."
                );
            }

            // สร้าง Token และปรับปรุง RefreshToken
            var token = _jwtTokenGenerator.GenerateToken(user);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            // บันทึกการเปลี่ยนแปลงในฐานข้อมูล
            await _unitOfWork.Users.UpdateAsync(user);
            var commitResult = await _unitOfWork.CommitAsync();

            if (!commitResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return Result<LoginResponse>.Failure($"Commit failed: {commitResult.Message}");
            }

            UserInfoResponse userInfo = new UserInfoResponse { Id = user.Id, Email = user.Email };
            // ส่งค่ากลับในรูปแบบ LoginResponse
            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = user.RefreshToken,
                User = userInfo,
            };

            return Result<LoginResponse>.Success(response, "Login successful.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            // บันทึกข้อผิดพลาดและส่งข้อความกลับ
            _logger.LogError(ex, "An error occurred during login.");
            return Result<LoginResponse>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles token refresh
    /// </summary>
    public async Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);
            if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return Result<LoginResponse>.Failure("Invalid or expired refresh token.");
            }

            var newToken = _jwtTokenGenerator.GenerateToken(user);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _unitOfWork.Users.UpdateAsync(user);
            var commitResult = await _unitOfWork.CommitAsync();

            if (!commitResult.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return Result<LoginResponse>.Failure($"Commit failed: {commitResult.Message}");
            }
            UserInfoResponse userInfo = new UserInfoResponse { Id = user.Id, Email = user.Email };

            var response = new LoginResponse
            {
                Token = newToken,
                RefreshToken = user.RefreshToken,
                User = userInfo,
            };

            return Result<LoginResponse>.Success(response, "Token refreshed successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            // บันทึกข้อผิดพลาดและส่งข้อความกลับ
            _logger.LogError(ex, "An error occurred during refresh token.");
            return Result<LoginResponse>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result<UserInfoResponse>> UserInfoAsync(Guid userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<UserInfoResponse>.Failure("User not found.");
            }

            var response = new UserInfoResponse { Email = user.Email };

            return Result<UserInfoResponse>.Success(response, "User info retrieved successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            // บันทึกข้อผิดพลาดและส่งข้อความกลับ
            _logger.LogError(ex, "An error occurred during getting user info.");
            return Result<UserInfoResponse>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a secure refresh token
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

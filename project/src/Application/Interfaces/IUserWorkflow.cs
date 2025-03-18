using Application.DTOs;
using Domain.Models;

namespace Application.Interfaces;

public interface IUserWorkflow
{
    Task<Result<object>> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken);
    Task<Result<UserInfoResponse>> UserInfoAsync(Guid userId);
}

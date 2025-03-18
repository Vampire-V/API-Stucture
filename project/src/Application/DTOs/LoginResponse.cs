namespace Application.DTOs;

public class LoginResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required UserInfoResponse User { get; set; }
}

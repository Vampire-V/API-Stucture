namespace Application.DTOs;

public class UserInfoResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
}

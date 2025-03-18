using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    string GenerateToken(User user);
}

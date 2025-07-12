using Entities.Models;

namespace Services.Interface
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        int? ValidateTokenAndGetUserId(string token);
    }

}
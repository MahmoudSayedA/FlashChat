using Api.Entities;

namespace Api.Application.Auth.Interfaces
{
    public interface ITokenGenerator
    {
        Task<string> GenerateJwtToken(User user);
        Task<string> GenerateRefreshToken();
        int TokenExpiryInMinutes { get; }
        int RefreshTokenExpiryInDays { get; }
    }
}

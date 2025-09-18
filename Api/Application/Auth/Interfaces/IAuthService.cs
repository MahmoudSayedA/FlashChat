using Api.Application.Auth.Dtos;
using Api.Application.Models;
using Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Application.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterDto model);
        Task<LoginResponseModel> LoginAsync(LoginDto model);
        Task<LoginResponseModel> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<UserInfoDto> GetUserInfoAsync(string userId);
        Task<UserInfoDto> GetMyInfo();
        Task<ICollection<string>> GetRolesAsync(User? user = null);
        Task CleanupOldRefreshTokensAsync();
    }
}

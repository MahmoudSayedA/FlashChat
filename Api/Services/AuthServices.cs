using Api.Application.Auth.Dtos;
using Api.Application.Auth.Interfaces;
using Api.Application.Models;
using Api.Data;
using Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly HttpContext? _httpContext;

        public AuthService(AppDbContext context, RoleManager<Role> roleManager, UserManager<User> userManager, ITokenGenerator tokenGenerator, IHttpContextAccessor httpContextAccessor)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
            _httpContext = httpContextAccessor.HttpContext;
            _context = context;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto model)
        {
            var user = new User { UserName = model.Username, Email = model.Email };
            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<LoginResponseModel> LoginAsync(LoginDto model)
        {
            // Manually authenticate the user
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                //await _userManager.AccessFailedAsync(user);
                throw new UnauthorizedAccessException("Invalid login attempt.");
            }
            //if (!user.EmailConfirmed)
            //{
            //    throw new UnauthorizedAccessException("Email is not confirmed.");
            //}
            ICollection<string> roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenGenerator.GenerateJwtToken(user);

            // check if refresh token exists for user, if yes, revoked it
            var existingTokens = _context.Set<RefreshToken>().Where(x => x.UserId == user.Id);
            foreach (var item in existingTokens)
            {
                item.Revoked = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            var refreshToken = await _tokenGenerator.GenerateRefreshToken();

            await SaveRefreshTokenAsync(user, refreshToken);

            return new LoginResponseModel
            {
                Token = token,
                TokenExpiryInMinutes = _tokenGenerator.TokenExpiryInMinutes,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Roles = roles,
            };
        }
        public async Task<ICollection<string>> GetRolesAsync(User? user = null)
        {
            if (user == null)
            {
                var res = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
                return res!;
            }

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<LoginResponseModel> RefreshTokenAsync(string token, CancellationToken cancellationToken)
        {

            // begin db transaction
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Find the refresh token in the database
                var refreshToken = await _context
                    .Set<RefreshToken>()
                    .FirstOrDefaultAsync(x => x.Token == token, cancellationToken)
                    ?? throw new UnauthorizedAccessException("Invalid token.");

                if (!refreshToken.IsActive)
                {
                    throw new UnauthorizedAccessException("Refresh token is expired or not active.");
                }

                // Generate a new access token and a new refresh token
                var user = await _context.Set<User>()
                    .FindAsync([refreshToken.UserId, cancellationToken], cancellationToken: cancellationToken)
                    ?? throw new UnauthorizedAccessException("User not found.");

                var newTokens = await _tokenGenerator.GenerateJwtToken(user);

                // commit db transaction
                await transaction.CommitAsync(cancellationToken);

                return new LoginResponseModel
                {
                    Token = newTokens,
                    TokenExpiryInMinutes = _tokenGenerator.TokenExpiryInMinutes,
                    RefreshToken = refreshToken.Token,
                    UserId = user.Id,
                    Roles = await GetRolesAsync(user)
                };

            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

        }

        public async Task<UserInfoDto> GetUserInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            return new UserInfoDto
            {
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                // Other user properties
            };
        }

        public Task<UserInfoDto> GetMyInfo()
        {
            var userId = _httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            return GetUserInfoAsync(userId);
        }

        public async Task SaveRefreshTokenAsync(User user, string refreshToken)
        {
            var token = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(_tokenGenerator.RefreshTokenExpiryInDays),
                UserId = user.Id
            };

            await _context.Set<RefreshToken>().AddAsync(token);
            await _context.SaveChangesAsync();
        }

        //  clean up old refresh tokens
        public async Task CleanupOldRefreshTokensAsync()
        {
            int deleteBeforeDays = 10; // TODO: move to config
            var thresholdDate = DateTime.UtcNow.AddDays(-deleteBeforeDays);

            await _context.Set<RefreshToken>()
                .Where(rt => rt.Expires < thresholdDate || (rt.Revoked != null && rt.Revoked < thresholdDate))
                .ExecuteDeleteAsync();
        }


    }
}

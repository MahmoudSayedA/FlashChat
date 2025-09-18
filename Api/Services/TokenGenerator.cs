using Api.Application.Auth.Interfaces;
using Api.Entities;
using Api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtOptions _options;

        public TokenGenerator(UserManager<User> userManager, IOptions<JwtOptions> options)
        {
            _userManager = userManager;
            _options = options.Value;
        }

        public int TokenExpiryInMinutes => _options.DurationInMinutes;

        public int RefreshTokenExpiryInDays => _options.RefreshTokenExpiryInDays;

        public async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            // Fetch roles and add them as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //// Add role claims to the token
            //foreach(var role in roles)
            //{
            //    var roleObj = await _roleManager.FindByNameAsync(role);
            //    var roleClaims = await _roleManager.GetClaimsAsync(roleObj!);
            //    claims.AddRange(roleClaims);
            //}

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_options.DurationInMinutes),
                signingCredentials: creds);

            return await Task.Run(() => new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<string> GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return await Task.Run(() => Convert.ToBase64String(randomNumber));
        }

    }
}

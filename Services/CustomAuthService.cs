using FamilyShoppingList.Data;
using FamilyShoppingList.DTO;
using FamilyShoppingList.Exceptions;
using FamilyShoppingList.Models;
using FamilyShoppingList.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamilyShoppingList.Services
{
    public class CustomAuthService : IAuthService
    {
        private readonly ShoppingListDbContext _db;
        private readonly PasswordHasher<User> _hasher;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        private static readonly TimeSpan RefreshTokenGracePeriod = TimeSpan.FromSeconds(2);
        public CustomAuthService(
            ShoppingListDbContext db,
            PasswordHasher<User> hasher,
            IConfiguration configuration)
        {
            _db = db;
            _hasher = hasher;

            _jwtKey = configuration["Jwt:Key"]!;
            _jwtIssuer = configuration["Jwt:Issuer"]!;
            _jwtAudience = configuration["Jwt:Audience"]!;
        }

        public string HashPassword(User user, string Hash)
        {

            return _hasher.HashPassword(user, Hash);
        }

        public bool VerifyHash(User user, string Hash)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, Hash);
            return result == PasswordVerificationResult.Success;
        }

        private string HashRefreshToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GeneratePlainToken()
        {
            return Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(64));
        }
        private RefreshToken GenerateRefreshToken(string plainToken, Guid userId)
        {
            return new RefreshToken
            {
                Token = HashRefreshToken(plainToken),

                UserId = userId,

                CreatedAt = DateTime.UtcNow,

                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
        }

        public async Task<UserRegisterResponse> Register(UserRegisterRequest registerRequest)
        {
            if (await _db.Users.AnyAsync(u => u.Username == registerRequest.Username))
            {
                throw new ConflictException(
                "Registration failed.",
                [
                    "The email is already registered."
                ]);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = HashPassword(new User
                {
                    Username = " ",
                    PasswordHash = " "
                }, registerRequest.Password)
            };
            _db.Users.Add(user);

            await _db.SaveChangesAsync();

            var userResponse = new UserRegisterResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            return userResponse;
        }

        public async Task<AuthTokenResponse> Login(UserLoginRequest loginRequest)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user is null ||
                VerifyHash(user, loginRequest.Password)
                == false)
            {
                throw new UnauthorizedException(
                "Login failed.",
                [
                    "Invalid email or password."
                ]);
            }

            string plainRefreshToken = GeneratePlainToken();

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(plainRefreshToken, user.Id);

            await _db.RefreshTokens.AddAsync(refreshToken);

            await _db.SaveChangesAsync();

            return new AuthTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = plainRefreshToken
            };

        }

        public async Task<AuthTokenResponse> Refresh(string refreshTokenFromCookie)
        {
            var hashedToken = HashRefreshToken(refreshTokenFromCookie);

            var dbRefreshToken = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == hashedToken);

            if (dbRefreshToken is null || dbRefreshToken.IsExpired)
            {
                throw new UnauthorizedException(
                "Unauthorized",
                [
                    "Unauthorized"
                ]);
            }
            if (dbRefreshToken.IsRevoked)
            {
                // Should never happen, but be defensive.
                if (dbRefreshToken.RevokedAt == null)
                {
                    throw new UnauthorizedException(
                    "Unauthorized",
                    [
                        "Unauthorized"
                    ]);
                }

                // Steal
                if (dbRefreshToken.ReuseDetected)
                {
                    throw new UnauthorizedException(
                    "Unauthorized",
                    [
                        "Unauthorized"
                    ]);
                }

                var revokedSince = DateTime.UtcNow - dbRefreshToken.RevokedAt.Value;

                // Duplicate refresh request (likely race condition)
                if (revokedSince <= RefreshTokenGracePeriod)
                {
                    throw new UnauthorizedException(
                    "Unauthorized",
                    [
                        "Unauthorized"
                    ]);
                }

                // Outside the grace period, suspicious refresh token reuse.

                dbRefreshToken.ReuseDetected = true;

                var tokens = await _db.RefreshTokens
                    .Where(r => r.UserId == dbRefreshToken.UserId &&
                                r.RevokedAt == null)
                    .ToListAsync();

                var now = DateTime.UtcNow;

                foreach (var token in tokens)
                {
                    token.RevokedAt = now;
                }

                await _db.SaveChangesAsync();
                throw new UnauthorizedException(
                "Unauthorized",
                [
                    "Unauthorized"
                ]);
            }

            var user = dbRefreshToken.User;

            if (user is null)
            {
                throw new UnauthorizedException(
                "Unauthorized",
                [
                    "Unauthorized"
                ]);
            }

            var newAccessToken = GenerateAccessToken(user);

            dbRefreshToken.RevokedAt = DateTime.UtcNow;

            var newPlainRefreshToken = GeneratePlainToken();

            var newRefreshToken = GenerateRefreshToken(newPlainRefreshToken, user.Id);

            _db.RefreshTokens.Add(newRefreshToken);

            await _db.SaveChangesAsync();

            return new AuthTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newPlainRefreshToken
            };

        }

        public async Task Logout(string refreshTokenFromCookie)
        {
            var hashedToken = HashRefreshToken(refreshTokenFromCookie);

            var dbRefreshToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == hashedToken);

            if (dbRefreshToken == null)
                return;

            if (!dbRefreshToken.IsRevoked)
                dbRefreshToken.RevokedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
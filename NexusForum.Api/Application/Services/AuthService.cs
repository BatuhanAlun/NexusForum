using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NexusForum.Api.Application.DTOs.Auth;
using NexusForum.Api.Application.Interfaces.Services;
using NexusForum.Api.Common.Results;
using NexusForum.Api.Domain.Entities;
using NexusForum.Api.Domain.Interfaces.Repositories;

namespace NexusForum.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRevokedTokenRepository _revokedTokenRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IRevokedTokenRepository revokedTokenRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _revokedTokenRepository = revokedTokenRepository;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            return Result<AuthResponse>.Failure("Email already in use.", 409);

        if (await _userRepository.ExistsByUsernameAsync(request.Username))
            return Result<AuthResponse>.Failure("Username already taken.", 409);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email.ToLowerInvariant(),
            // BCrypt work factor 12 is the recommended baseline for 2024+.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return Result<AuthResponse>.Success(
            new AuthResponse(user.Id, user.Username, user.Email, user.Role.ToString(), GenerateJwt(user)),
            201);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // Always verify hash even when user not found to prevent timing attacks.
        var dummyHash = "$2a$12$invalidhashusedtoblindtimingattacks000000000000000000000";
        var hashToVerify = user?.PasswordHash ?? dummyHash;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, hashToVerify) || user is null)
            return Result<AuthResponse>.Failure("Invalid email or password.", 401);

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Account is disabled.", 403);

        return Result<AuthResponse>.Success(
            new AuthResponse(user.Id, user.Username, user.Email, user.Role.ToString(), GenerateJwt(user)));
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"]));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            // Use short "role" not ClaimTypes.Role — ClaimTypes.Role is the long MS URI which is non-standard in JWTs.
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task LogoutAsync(string jti, DateTime tokenExpiry)
    {
        await _revokedTokenRepository.RevokeAsync(jti, tokenExpiry);
        await _revokedTokenRepository.SaveChangesAsync();
    }
}

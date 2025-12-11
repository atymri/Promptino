using Microsoft.Extensions.Options;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using Promptino.Core.Options;
using Promptino.Core.ServiceContracts;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;
using AutoMapper;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Promptino.Core.Services;

internal class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(
        IOptions<JwtOptions> jwtOptions,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _jwtOptions = jwtOptions.Value;
        _mapper = mapper;
        _userManager = userManager;
    }

    public AuthResponse CreateToken(ApplicationUser user)
    {
        var roles = _userManager.GetRolesAsync(user).Result;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Email)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var signinCreds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryInMinutes),
            signingCredentials: signinCreds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return _mapper.Map<AuthResponse>(user) with
        {
            LastLoginAt = DateTime.UtcNow,
            Token = token,
            IsLockedOut = user.LockoutEnd.HasValue,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpiryInMinutes),
            IsAdmin = roles.Contains("Admin")
        };
    }

    public ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenValidator = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
        };

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token, tokenValidator, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var range = RandomNumberGenerator.Create();
        range.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

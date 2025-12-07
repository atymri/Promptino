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

namespace Promptino.Core.Services;

internal class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IMapper _mapper;
    public TokenService(IOptions<JwtOptions> jwtOptions, IMapper mapper)
    {
        _jwtOptions = jwtOptions.Value;
        _mapper = mapper;
    }

    public AuthResponse CreateToken(ApplicationUser user)
    {
        var claims = new Claim[]
        {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email),
            new (ClaimTypes.Name, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var signinCreds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenGenerator = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtOptions.ExpiryInMinutes),
            signingCredentials: signinCreds);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenGenerator);

        var response = _mapper.Map<AuthResponse>(user) 
            with { LastLoginAt = DateTime.UtcNow,
            Token = token, 
            IsLockedOut = user.LockoutEnd.HasValue,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtOptions.RefreshTokenExpiryInMinutes))};

        return response;
    }

    public ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenValidator = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // its being called with an expired token, so we don't validate lifetime here
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
        };

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token, tokenValidator, out var securityToken);

        if(securityToken is not JwtSecurityToken jwtSecurityToken ||
           !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

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

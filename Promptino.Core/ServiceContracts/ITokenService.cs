using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System.Security.Claims;

namespace Promptino.Core.ServiceContracts;

public interface ITokenService
{
    public AuthResponse CreateToken(ApplicationUser user);
    public ClaimsPrincipal GetPrincipalFromToken(string token);
    public string GenerateRefreshToken();
}

using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts;

public interface ITokenService
{
    public AuthResponse CreateToken(ApplicationUser user);
}

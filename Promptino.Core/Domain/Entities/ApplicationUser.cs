using Microsoft.AspNetCore.Identity;

namespace Promptino.Core.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<FavoritePrompts> FavoritePrompts { get; set; } = new();
    public int LockoutMultiplier { get; set; } = 1;
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}

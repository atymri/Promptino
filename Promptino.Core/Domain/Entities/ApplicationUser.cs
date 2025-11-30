using Microsoft.AspNetCore.Identity;

namespace Promptino.Core.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<FavoritePrompts> FavoritePrompts { get; set; } = new();
}

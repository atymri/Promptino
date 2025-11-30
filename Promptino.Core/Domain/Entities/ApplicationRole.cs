using Microsoft.AspNetCore.Identity;

namespace Promptino.Core.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Details { get; set; }
}


using Microsoft.AspNetCore.Identity;

namespace Promptino.Core.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<SavedPrompt> SavedPrompts { get; set; } = new();
    public List<Prompt> Prompts { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public List<PromptReaction> Reactions { get; set; } = new();
    public List<PromptReport> ReportsFiled { get; set; } = new();
    public int LockoutMultiplier { get; set; } = 1;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }

    // Rotation: the token that was current before the latest refresh.
    // Presenting it again is evidence of token theft (reuse detection).
    public string? PreviousRefreshToken { get; set; }
    public DateTime? PreviousRefreshTokenExpiration { get; set; }
}

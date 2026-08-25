using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public class Prompt : BaseEntity<Guid>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserID { get; set; }

    // Moderation: hidden prompts are excluded from public listings but remain
    // visible to their owner and admins (soft-hide, reversible)
    public bool IsHidden { get; set; }

    public ApplicationUser User { get; set; }
    public virtual List<PromptImage> PromptImages { get; set; } = new();
    public virtual List<SavedPrompt> SavedPrompts { get; set; } = new();
    public virtual List<PromptCategories> PromptCategories { get; set; } = new();
    public virtual List<Comment> Comments { get; set; } = new();
    public virtual List<PromptReaction> Reactions { get; set; } = new();
}

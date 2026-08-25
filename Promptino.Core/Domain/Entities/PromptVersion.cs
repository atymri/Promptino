using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

// Immutable snapshot of a prompt's content, taken before each update
public class PromptVersion : BaseEntity<Guid>
{
    [ForeignKey(nameof(Prompt))]
    public Guid PromptID { get; set; }

    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid EditedByUserID { get; set; }

    public Prompt Prompt { get; set; } = null!;
}

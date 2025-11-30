using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public class FavoritePrompts : BaseEntity<Guid>
{
    [ForeignKey(nameof(User))]
    public Guid UserID { get; set; }
    [ForeignKey(nameof(Prompt))]
    public Guid PromptID { get; set; }

    public ApplicationUser User { get; set; }
    public Prompt Prompt { get; set; }
}


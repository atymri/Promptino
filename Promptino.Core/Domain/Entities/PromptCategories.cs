using System.ComponentModel.DataAnnotations.Schema;
using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.Entities;

public class PromptCategories : BaseEntity<Guid>
{
    [ForeignKey(nameof(Prompt))]
    public Guid PromptId { get; set; }
    [ForeignKey(nameof(Category))]
    public Guid CategoryId { get; set; }
    public virtual Prompt Prompt { get; set; }
    public virtual Category Category { get; set; }

}

using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public class PromptImage : BaseEntity<Guid>
{
    [ForeignKey(nameof(Prompt))]
    public Guid PromptID { get; set; }
    [ForeignKey(nameof(Image))]
    public Guid ImageID { get; set; }
    public virtual Image Image { get; set; }
    public virtual Prompt Prompt { get; set; }
}
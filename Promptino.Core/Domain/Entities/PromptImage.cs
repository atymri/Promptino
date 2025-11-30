using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public class PromptImage : BaseEntity<Guid>
{
    [ForeignKey(nameof(Prompt))]
    public Guid PromptID { get; set; }
    [ForeignKey(nameof(Image))]
    public Guid ImageID { get; set; }
    public Image Image { get; set; }
    public Prompt Prompt { get; set; }
}
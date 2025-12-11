using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promptino.Core.Domain.Entities;

public class PromptCategories : BaseEntity<Guid>
{
    [ForeignKey(nameof(Prompt)]
    public Guid PromptId { get; set; }
    [ForeignKey(nameof(Prompt))]
    public Guid CategoryId { get; set; }
    public virtual Prompt Prompt { get; set; } = new();
    public virtual Category Category { get; set; } = new();

}

namespace Promptino.Core.Domain.Entities;

public class Category : BaseEntity<Guid>
{
    public string Title { get; set; }
    public string Description { get; set; }

    public virtual List<PromptCategories> PromptCategories { get; set; } = new();
}

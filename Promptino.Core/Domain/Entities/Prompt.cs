namespace Promptino.Core.Domain.Entities;

public class Prompt : BaseEntity<Guid>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public virtual List<PromptImage> PromptImages { get; set; } = new();
    public virtual List<FavoritePrompts> FavoritePrompts { get; set; } = new();
}


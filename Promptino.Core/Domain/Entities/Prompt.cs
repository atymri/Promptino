namespace Promptino.Core.Domain.Entities;

public class Prompt : BaseEntity<Guid>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public List<PromptImage> PromptImages { get; set; } = new();
    public List<FavoritePrompts> FavoritePrompts { get; set; } = new();
}


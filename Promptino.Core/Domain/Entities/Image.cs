namespace Promptino.Core.Domain.Entities;

public class Image : BaseEntity<Guid>
{
    public string Path { get; set; }
    public string Title { get; set; }
    public string GeneratedWith { get; set; }
    public virtual List<PromptImage> PromptImages { get; set; } = new();
}
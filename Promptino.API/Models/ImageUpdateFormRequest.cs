namespace Promptino.API.Models;

public class ImageUpdateFormRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public IFormFile? File { get; set; }
    public string GeneratedWith { get; set; }
    public string Path { get; set; }
}

namespace Promptino.Core.DTOs;

public record PromptAddRequest(
    string Title,
    string Description,
    string Content
)
{
    public PromptAddRequest() : this(default, default, default)
    { }
};

public record PromptUpdateRequest(
    Guid Id,
    string Title,
    string Description,
    string Content
)
{
    public PromptUpdateRequest() : this(default, default ,default, default)
    { }    
};

public record PromptResponse(
    Guid Id,
    string Title,
    string Description,
    string Content,
    DateTime DateAdded,
    DateTime LastUpdate,
    IEnumerable<ImageResponse>? Images = null
)
{
    public PromptResponse() : this(default, default, default, default, default, default, default)
    { }
};
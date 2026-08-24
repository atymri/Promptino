namespace Promptino.Core.DTOs;

public record SavedPromptAddRequest(
    Guid PromptID
)
{
    public SavedPromptAddRequest() : this(default(Guid))
    { }
};

public record SavedPromptResponse(
    Guid Id,
    Guid UserId,
    Guid PromptId,
    string PromptTitle,
    string PromptDescription,
    DateTime CreatedAt
)
{
    public SavedPromptResponse() : this(default, default, default, default, default, default)
    { }
};

public record SavedWithDetailsResponse(
    Guid SavedId,
    PromptResponse Prompt,
    DateTime CreatedAt
)
{
    public SavedWithDetailsResponse() : this(default, default, default)
    { }
};

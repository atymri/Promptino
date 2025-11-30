namespace Promptino.Core.DTOs;

public record FavoritePromptAddRequest(
    Guid UserID,
    Guid PromptID
)
{
    public FavoritePromptAddRequest() : this(default, default)
    {}
};

public record FavoritePromptResponse(
    Guid Id,
    Guid UserId,
    Guid PromptId,
    string PromptTitle,
    string PromptDescription,
    DateTime CreatedAt
)
{
    public FavoritePromptResponse() : this(default, default, default, default, default, default)
    { }
};

// /api/users/{userId}/fav_prompts/
public record FavoriteWithDetailsResponse(
    Guid FavoriteId,
    PromptResponse Prompt,
    DateTime CreatedAt
)
{
    public FavoriteWithDetailsResponse() : this(default, default ,default)
    { }
};
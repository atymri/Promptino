using Promptino.Core.Domain.Entities;

namespace Promptino.Core.DTOs;

public record ReactionAddRequest(
    ReactionType Type
)
{
    public ReactionAddRequest() : this(default(ReactionType))
    { }
};

public record ReactionStateResponse(
    int LikesCount,
    int DislikesCount,
    ReactionType? MyReaction
)
{
    public ReactionStateResponse() : this(default, default, default)
    { }
};

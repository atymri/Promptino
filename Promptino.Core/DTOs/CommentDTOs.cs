namespace Promptino.Core.DTOs;

public record CommentAddRequest(
    Guid PromptID,
    string Content,
    Guid? ParentCommentID = null
)
{
    public CommentAddRequest() : this(default, default)
    { }
};

public record CommentResponse(
    Guid Id,
    Guid UserId,
    string AuthorName,
    Guid PromptId,
    string Content,
    DateTime CreatedAt,
    Guid? ParentCommentID = null,
    int LikesCount = 0,
    bool IsLikedByMe = false,
    IEnumerable<CommentResponse>? Replies = null
)
{
    public CommentResponse() : this(default, default, default, default, default, default)
    { }
};

public record CommentLikeStateResponse(
    int LikesCount,
    bool IsLikedByMe
)
{
    public CommentLikeStateResponse() : this(default, default)
    { }
};

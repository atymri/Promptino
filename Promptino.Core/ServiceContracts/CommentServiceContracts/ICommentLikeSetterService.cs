using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.CommentServiceContracts;

public interface ICommentLikeSetterService
{
    Task<CommentLikeStateResponse> ToggleLikeAsync(Guid userId, Guid promptId, Guid commentId);
}

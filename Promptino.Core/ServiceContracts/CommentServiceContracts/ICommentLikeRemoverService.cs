using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.CommentServiceContracts;

public interface ICommentLikeRemoverService
{
    Task<CommentLikeStateResponse> RemoveLikeAsync(Guid userId, Guid promptId, Guid commentId);
}

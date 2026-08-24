using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface ICommentLikeRepository
{
    Task<CommentLike?> GetLikeAsync(Guid userId, Guid commentId);
    Task AddLikeAsync(CommentLike like);
    Task<bool> RemoveLikeAsync(Guid userId, Guid commentId);
    Task<int> GetCountAsync(Guid commentId);
}

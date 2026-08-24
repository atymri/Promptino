using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface ICommentRepository
{
    Task<(int TotalCount, IReadOnlyList<Comment> Items)> GetRootsByPromptPagedAsync(Guid promptId, int page, int pageSize);
    Task<IReadOnlyList<Comment>> GetRepliesForRootsAsync(IReadOnlyList<Guid> rootIds);
    Task<Comment?> GetByIdAsync(Guid commentId);
    Task<Comment> AddAsync(Comment comment);
    Task<bool> DeleteAsync(Guid commentId);
}

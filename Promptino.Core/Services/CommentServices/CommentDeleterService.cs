using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.CommentServiceContracts;

namespace Promptino.Core.Services.CommentServices;

public class CommentDeleterService : ICommentDeleterService
{
    private readonly ICommentRepository _commentRepository;

    public CommentDeleterService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid currentUserId, bool isAdmin)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment is null)
            throw new CommentNotFoundException("نظر مورد نظر پیدا نشد");

        if (!isAdmin && comment.UserID != currentUserId)
            throw new CommentOwnershipException("شما اجازه حذف این نظر را ندارید");

        return await _commentRepository.DeleteAsync(commentId);
    }
}

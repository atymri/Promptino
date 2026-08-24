using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.CommentServiceContracts;

namespace Promptino.Core.Services.CommentServices;

public class CommentLikeRemoverService : ICommentLikeRemoverService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _likeRepository;

    public CommentLikeRemoverService(
        ICommentRepository commentRepository,
        ICommentLikeRepository likeRepository)
    {
        _commentRepository = commentRepository;
        _likeRepository = likeRepository;
    }

    public async Task<CommentLikeStateResponse> RemoveLikeAsync(Guid userId, Guid promptId, Guid commentId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment is null || comment.PromptID != promptId)
            throw new CommentNotFoundException("نظر مورد نظر پیدا نشد");

        await _likeRepository.RemoveLikeAsync(userId, commentId);

        var count = await _likeRepository.GetCountAsync(commentId);
        return new CommentLikeStateResponse(count, false);
    }
}

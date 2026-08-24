using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.CommentServiceContracts;

namespace Promptino.Core.Services.CommentServices;

public class CommentLikeSetterService : ICommentLikeSetterService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _likeRepository;

    public CommentLikeSetterService(
        ICommentRepository commentRepository,
        ICommentLikeRepository likeRepository)
    {
        _commentRepository = commentRepository;
        _likeRepository = likeRepository;
    }

    public async Task<CommentLikeStateResponse> ToggleLikeAsync(Guid userId, Guid promptId, Guid commentId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment is null || comment.PromptID != promptId)
            throw new CommentNotFoundException("نظر مورد نظر پیدا نشد");

        var existing = await _likeRepository.GetLikeAsync(userId, commentId);

        if (existing is null)
        {
            await _likeRepository.AddLikeAsync(
                new CommentLike { UserID = userId, CommentID = commentId });
        }
        else
        {
            // clicking again un-likes
            await _likeRepository.RemoveLikeAsync(userId, commentId);
        }

        return await BuildStateAsync(userId, commentId);
    }

    private async Task<CommentLikeStateResponse> BuildStateAsync(Guid userId, Guid commentId)
    {
        var count = await _likeRepository.GetCountAsync(commentId);
        var mine = await _likeRepository.GetLikeAsync(userId, commentId);
        return new CommentLikeStateResponse(count, mine is not null);
    }
}

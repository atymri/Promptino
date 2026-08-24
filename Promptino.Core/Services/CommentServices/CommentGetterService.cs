using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.CommentServiceContracts;

namespace Promptino.Core.Services.CommentServices;

public class CommentGetterService : ICommentGetterService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;

    public CommentGetterService(ICommentRepository commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CommentResponse>> GetCommentsForPromptAsync(Guid promptId, Guid? currentUserId = null, int page = 1, int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = PaginationDefaults.DefaultPageSize;
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;

        var (totalCount, roots) = await _commentRepository.GetRootsByPromptPagedAsync(promptId, page, pageSize);
        var replies = roots.Count > 0
            ? await _commentRepository.GetRepliesForRootsAsync(roots.Select(r => r.ID).ToList())
            : [];

        // write-time normalization guarantees replies are at most one level deep
        var repliesByRoot = replies
            .Where(c => c.ParentCommentID.HasValue)
            .GroupBy(c => c.ParentCommentID.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        CommentResponse Enrich(Comment entity)
        {
            var likedByMe = currentUserId.HasValue
                && entity.Likes != null
                && entity.Likes.Any(l => l.UserID == currentUserId.Value);

            var response = _mapper.Map<CommentResponse>(entity) with
            {
                LikesCount = entity.Likes?.Count ?? 0,
                IsLikedByMe = likedByMe,
                Replies = repliesByRoot.TryGetValue(entity.ID, out var entityReplies)
                    ? entityReplies.Select(Enrich).ToList()
                    : Enumerable.Empty<CommentResponse>()
            };
            return response;
        }

        return new PagedResult<CommentResponse>(
            roots.Select(Enrich).ToList(), page, pageSize, totalCount);
    }
}

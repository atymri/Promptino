using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.CommentServiceContracts;

namespace Promptino.Core.Services.CommentServices;

public class CommentAdderService : ICommentAdderService
{
    private readonly IPromptRepository _promptRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;

    public CommentAdderService(
        IPromptRepository promptRepository,
        ICommentRepository commentRepository,
        IMapper mapper)
    {
        _promptRepository = promptRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<CommentResponse> AddCommentAsync(Guid userId, CommentAddRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        if (!await _promptRepository.DoesPromptExistAsync(request.PromptID))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر پیدا نشد");

        // replies are one level deep: a reply to a reply attaches to the same root
        if (request.ParentCommentID.HasValue)
        {
            var parent = await _commentRepository.GetByIdAsync(request.ParentCommentID.Value);
            if (parent is null || parent.PromptID != request.PromptID)
                throw new CommentNotFoundException("نظر والد پیدا نشد");

            request = request with { ParentCommentID = parent.ParentCommentID ?? parent.ID };
        }

        var comment = _mapper.Map<Comment>(request);
        comment.UserID = userId;

        var added = await _commentRepository.AddAsync(comment);
        return _mapper.Map<CommentResponse>(added);
    }
}

using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.CommentServiceContracts;

public interface ICommentGetterService
{
    Task<PagedResult<CommentResponse>> GetCommentsForPromptAsync(Guid promptId, Guid? currentUserId = null, int page = 1, int pageSize = PaginationDefaults.DefaultPageSize);
}

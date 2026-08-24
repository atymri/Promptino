using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.CommentServiceContracts;

public interface ICommentAdderService
{
    Task<CommentResponse> AddCommentAsync(Guid userId, CommentAddRequest request);
}

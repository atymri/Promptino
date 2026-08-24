namespace Promptino.Core.ServiceContracts.CommentServiceContracts;

public interface ICommentDeleterService
{
    Task<bool> DeleteCommentAsync(Guid commentId, Guid currentUserId, bool isAdmin);
}

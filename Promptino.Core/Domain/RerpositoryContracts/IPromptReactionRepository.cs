using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface IPromptReactionRepository
{
    Task<PromptReaction?> GetReactionAsync(Guid userId, Guid promptId);
    Task AddReactionAsync(PromptReaction reaction);
    Task UpdateReactionAsync(PromptReaction reaction);
    Task<bool> RemoveReactionAsync(Guid userId, Guid promptId);
    Task<(int Likes, int Dislikes)> GetCountsAsync(Guid promptId);
}

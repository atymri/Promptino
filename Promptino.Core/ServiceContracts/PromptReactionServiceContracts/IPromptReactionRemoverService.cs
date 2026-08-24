using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.PromptReactionServiceContracts;

public interface IPromptReactionRemoverService
{
    Task<ReactionStateResponse> RemoveReactionAsync(Guid userId, Guid promptId);
}

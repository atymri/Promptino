using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.PromptReactionServiceContracts;

public interface IPromptReactionGetterService
{
    Task<ReactionStateResponse> GetStateAsync(Guid userId, Guid promptId);
}

using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.PromptReactionServiceContracts;

public interface IPromptReactionSetterService
{
    Task<ReactionStateResponse> SetReactionAsync(Guid userId, Guid promptId, ReactionType reaction);
}

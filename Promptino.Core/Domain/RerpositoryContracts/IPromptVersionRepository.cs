using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.RerpositoryContracts;

public interface IPromptVersionRepository
{
    Task<int> GetNextVersionNumberAsync(Guid promptId);
    Task AddAsync(PromptVersion version);
    Task<IReadOnlyList<PromptVersion>> GetForPromptAsync(Guid promptId);
    Task<PromptVersion?> GetAsync(Guid promptId, int versionNumber);
}

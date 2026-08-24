using AutoMapper;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.SavedPromptServiceContracts;

namespace Promptino.Core.Services.SavedPromptServices;

public class SavedPromptGetterService : ISavedPromptGetterService
{
    private readonly ISavedPromptRepository _savedPromptRepository;
    private readonly IMapper _mapper;

    public SavedPromptGetterService(ISavedPromptRepository savedPromptRepository, IMapper mapper)
    {
        _savedPromptRepository = savedPromptRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SavedWithDetailsResponse>> GetSavedPromptsAsync(Guid userId)
    {
        var saved = await _savedPromptRepository.GetSavedByUserAsync(userId);
        return _mapper.Map<List<SavedWithDetailsResponse>>(saved);
    }

    public async Task<int> GetSavedCountAsync(Guid promptId)
        => await _savedPromptRepository.GetSavedCountAsync(promptId);

    public async Task<bool> IsSavedAsync(Guid userId, Guid promptId)
        => await _savedPromptRepository.IsSavedAsync(userId, promptId);
}

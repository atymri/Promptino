using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;


namespace Promptino.Core.Services.PromptServices;

public class PromptUpdaterService : IPromptUpdaterService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IPromptVersionRepository _versionRepository;
    private readonly IMapper _mapper;

    public PromptUpdaterService(
        IPromptRepository promptRepository,
        IPromptVersionRepository versionRepository,
        IMapper mapper)
    {
        _promptRepository = promptRepository;
        _versionRepository = versionRepository;
        _mapper = mapper;
    }

    public async Task<PromptResponse?> UpdatePromptAsync(PromptUpdateRequest promptRequest, Guid currentUserId, bool isAdmin)
    {
        if (promptRequest is null)
            throw new NullPromptRequestException(nameof(promptRequest));

        var existingPrompt = await _promptRepository.GetPromptByConditionAsync(p => p.ID == promptRequest.Id);
        if (existingPrompt is null)
            throw new PromptNotFoundExceptions("پرامپت مورد نظر وجود ندارد");

        if (!isAdmin && existingPrompt.UserID != currentUserId)
            throw new PromptOwnershipException("شما اجازه ویرایش این پرامپت را ندارید");

        // Snapshot the outgoing state before it is overwritten
        var version = new PromptVersion
        {
            PromptID = existingPrompt.ID,
            VersionNumber = await _versionRepository.GetNextVersionNumberAsync(existingPrompt.ID),
            Title = existingPrompt.Title,
            Description = existingPrompt.Description,
            Content = existingPrompt.Content,
            EditedByUserID = currentUserId
        };
        await _versionRepository.AddAsync(version);

        var prompt = _mapper.Map<Prompt>(promptRequest);
        var res = await _promptRepository.UpdatePromptAsync(prompt);
        return _mapper.Map<PromptResponse>(res);
    }
}

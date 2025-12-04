using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;


namespace Promptino.Core.Services.PromptServices;

public class PromptUpdaterService : IPromptUpdaterService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IMapper _mapper;
    public PromptUpdaterService(IPromptRepository promptRepository, IMapper mapper)
    {
        _promptRepository = promptRepository;
        _mapper = mapper;
    }
    public async Task<PromptResponse?> UpdatePromptAsync(PromptUpdateRequest promptRequest)
    {
        if (!await _promptRepository.DoesPromptExistAsync(promptRequest.Id))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر وجود ندارد");

        var prompt = _mapper.Map<Prompt>(promptRequest);
        var res = await _promptRepository.UpdatePromptAsync(prompt);
        return _mapper.Map<PromptResponse>(res);
    }
}

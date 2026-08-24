using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;

namespace Promptino.Core.Services.PromptServices;

public class PromptAdderService : IPromptAdderService
{
    private readonly IPromptImageRepository _promptImageRepository;
    private readonly IPromptRepository _promptRepository;
    private readonly IMapper _mapper;

    public PromptAdderService(
        IPromptImageRepository promptImageRepository,
        IPromptRepository promptRepository,
        IMapper mapper)
    {
        _promptImageRepository = promptImageRepository;
        _promptRepository = promptRepository;
        _mapper = mapper;
    }

    public async Task<PromptResponse> CreatePromptAsync(PromptAddRequest promptRequest, Guid ownerId)
    {
        if (promptRequest is null)
            throw new NullPromptRequestException(nameof(promptRequest));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("آیدی مالک پرامپت نمیتواند خالی باشد", nameof(ownerId));

        var prompt = _mapper.Map<Prompt>(promptRequest);
        prompt.UserID = ownerId;

        var res = await _promptRepository.AddPromptAsync(prompt);

        return _mapper.Map<PromptResponse>(res);
    }
}

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

    public async Task<FavoritePromptResponse> AddToFavoritesAsync(FavoritePromptAddRequest favoriteRequest)
    {
        var favPrompt = _mapper.Map<FavoritePrompts>(favoriteRequest);
        var success = await _promptRepository.AddToFavoritesAsync(favPrompt);

        if (!success)
            throw new Exception("Failed to add to favorites");

        return _mapper.Map<FavoritePromptResponse>(favPrompt);
    }

    public async Task<PromptResponse> CreatePromptAsync(PromptAddRequest promptRequest)
    {
        if (promptRequest.Images != null && promptRequest.Images.Count() > 6)
            throw new ImageLimitException("هر پرامپت میتواند حداکثر 6 تصویر ذخیره کند");

        return await _promptImageRepository.CreatePromptWithImagesAsync(promptRequest);
    }
}
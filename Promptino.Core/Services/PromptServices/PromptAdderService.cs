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
        if (favoriteRequest is null)
            throw new NullPromptRequestException(nameof(favoriteRequest));

        if (await _promptRepository.IsFavoriteAsync(favoriteRequest.UserID, favoriteRequest.PromptID))
            throw new PromptExistsException("پرامپت مورد نظر در حال حاضر در علاقه مندی های شما وجود دارد");

        if(!await _promptRepository.DoesPromptExistAsync(favoriteRequest.PromptID))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر پیدا نشد");

        var favPrompt = _mapper.Map<FavoritePrompts>(favoriteRequest);
        var success = await _promptRepository.AddToFavoritesAsync(favPrompt);

        if (!success)
            throw new Exception("خطا در افزودن پرامپت به مورد علاقه ها");

        return _mapper.Map<FavoritePromptResponse>(favPrompt);
    }

    public async Task<PromptResponse> CreatePromptAsync(PromptAddRequest promptRequest)
    {

        if (promptRequest is null)
            throw new NullPromptRequestException(nameof(promptRequest));

        //if (promptRequest.Images != null && promptRequest.Images.Count() > 6)
        //    throw new ImageLimitException("هر پرامپت میتواند حداکثر 6 تصویر ذخیره کند");

        // NOTE: more login may be added later, for now we are just saving the prompt in here.
        //return await _promptImageRepository.CreatePromptWithImagesAsync(promptRequest);

        var prompt = _mapper.Map<Prompt>(promptRequest);
        var res = await _promptRepository.AddPromptAsync(prompt);

        return _mapper.Map<PromptResponse>(res);
    }
}
using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;

namespace Promptino.Core.Services.ImageServices;

public class ImageAdderService : IImageAdderService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IMapper _mapper;

    public ImageAdderService(IImageRepository imageRepository,
        IPromptRepository promptRepository,
        IMapper mapperr)
    {
        _imageRepository = imageRepository;
        _promptRepository = promptRepository;
        _mapper = mapperr;
    }

    public async Task<bool> AddImageToPromptAsync(Guid promptId, Guid imageId)
    {
        var prompt = await _promptRepository.GetPromptByConditionAsync(p => p.ID == promptId);
        if (prompt is null)
            throw new PromptNotFoundExceptions("پرامپت مورد نظر وجود ندارد");

        if (!await _imageRepository.DoesImageExistAsync(imageId))
            throw new ImageNotFoundException("تصویر مورد نظر وجود ندارد");

        if (prompt.PromptImages != null && prompt.PromptImages.Count() > 6)
            throw new ImageLimitException("هر پرامپت میتواند حداکثر 6 تصویر ذخیره کند");


        return await _imageRepository.AddImageToPromptAsync(promptId, imageId);
    }

    public async Task<ImageResponse> CreateImageAsync(ImageAddRequest imageRequest)
    {
        if (imageRequest is null)
            throw new NullImageRequestException(nameof(imageRequest));

        var image = _mapper.Map<Image>(imageRequest);
        var res = await _imageRepository.AddImageAsync(image);
        return _mapper.Map<ImageResponse>(res);
    }

}
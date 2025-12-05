using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;

namespace Promptino.Core.Services.ImageServices;

public class ImageUpdaterService : IImageUpdaterService
{
    private readonly IImageRepository _imageRepository;
    private readonly IMapper _mapper;

    public ImageUpdaterService(IImageRepository imageRepository, IMapper mapper)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
    }

    public async Task<ImageResponse?> UpdateImageAsync(ImageUpdateRequest imageRequest)
    {
        if (!await _imageRepository.DoesImageExistAsync(imageRequest.Id))
            throw new ImageNotFoundException("تصویر مورد نظر وجود ندارد");

        var image = _mapper.Map<Image>(imageRequest);
        var res = await _imageRepository.UpdateImageAsync(image);

        return _mapper.Map<ImageResponse>(res);
    }
}
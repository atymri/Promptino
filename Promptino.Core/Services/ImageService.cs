using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts;
using System.Linq.Expressions;

namespace Promptino.Core.Services;

public class ImageService : IImageService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IMapper _mapper;

    public ImageService(IImageRepository imageRepository,
        IPromptRepository promptRepository,
        IMapper mapper)
    {
        _imageRepository = imageRepository;
        _promptRepository = promptRepository;
        _mapper = mapper;
    }

    public async Task<bool> AddImageToPromptAsync(Guid promptId, Guid imageId)
    {
        var prompt = await _promptRepository.GetPromptByConditionAsync(p => p.ID == promptId);
        if (prompt is null)
            throw new PromptNotFoundExceptions("پرامپت مورد نظر وجود ندارد");

        if (!_imageRepository.DoesImageExist(imageId))
            throw new ImageNotFoundException("تصویر مورد نظر وجود ندارد");

        if (prompt.PromptImages.Count == 6)
            throw new ImageLimitException("هر پرامپت میتواند حداکثر 6 تصویر ذخیره کند");

        return await _imageRepository.AddImageToPromptAsync(promptId, imageId);
    }

    public async Task<ImageResponse> CreateImageAsync(ImageAddRequest imageRequest)
    {
        var image = _mapper.Map<Image>(imageRequest);
        var res = await _imageRepository.AddImageAsync(image);
        return _mapper.Map<ImageResponse>(res);
    }

    public async Task<bool> DeleteImageAsync(Guid id)
    {
        if (!_imageRepository.DoesImageExist(id))
            throw new ImageNotFoundException("تصویر مورد نظر وجود ندارد");

        return await _imageRepository.DeleteImageAsync(id);
    }

    public async Task<IEnumerable<ImageResponse>> GetAllImagesAsync()
    {
        var images = await _imageRepository.GetImagesAsync();
        return _mapper.Map<List<ImageResponse>>(images);
    }

    public async Task<ImageResponse> GetImageByConditionAsync(Expression<Func<ImageResponse, bool>> condition)
    {
        var mappedExpression = _mapper.MapExpression<Expression<Func<Image, bool>>>(condition);
        var res = await _imageRepository.GetImageByConditionAsync(mappedExpression);

        if (res == null)
            throw new ImageNotFoundException("تصویر مورد نظر یافت نشد");

        return _mapper.Map<ImageResponse>(res);
    }

    public async Task<IEnumerable<ImageResponse>> GetImagesByConditionAsync(Expression<Func<ImageResponse, bool>> condition)
    {
        var mappedExpression = _mapper.MapExpression<Expression<Func<Image, bool>>>(condition);
        var res = await _imageRepository.GetImagesByConditionAsync(mappedExpression);
        return _mapper.Map<List<ImageResponse>>(res);
    }

    public async Task<IEnumerable<ImageResponse>> GetImagesByPromptIdAsync(Guid promptId)
    {
        var images = await _imageRepository.GetImagesByPromptIdAsync(promptId);
        return _mapper.Map<List<ImageResponse>>(images);
    }

    public async Task<bool> RemoveImageFromPromptAsync(Guid promptId, Guid imageId)
    {
        if (!_promptRepository.DoesPromptExist(promptId))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر یافت نشد");

        if (!_imageRepository.DoesImageExist(imageId))
            throw new ImageNotFoundException("تصویر مورد نظر یافت نشد");

        return await _imageRepository.RemoveImageFromPromptAsync(promptId, imageId);
    }

    public async Task<ImageResponse?> UpdateImageAsync(ImageUpdateRequest imageRequest)
    {
        if (!_imageRepository.DoesImageExist(imageRequest.Id))
            throw new ImageNotFoundException("تصویر مورد نظر وجود ندارد");

        var image = _mapper.Map<Image>(imageRequest);
        var res = await _imageRepository.UpdateImageAsync(image);

        return _mapper.Map<ImageResponse>(res);
    }
}
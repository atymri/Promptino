using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using System.Linq.Expressions;

namespace Promptino.Core.Services.ImageServices;

public class ImageGetterService : IImageGetterrService
{
    private readonly IImageRepository _imageRepository;
    private readonly IMapper _mapper;

    public ImageGetterService(IImageRepository imageRepository, IMapper mapper)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
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

}
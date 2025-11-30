using Promptino.Core.DTOs;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts;

public interface IImageService
{
    Task<IEnumerable<ImageResponse>> GetAllImagesAsync();
    Task<IEnumerable<ImageResponse>> GetImagesByConditionAsync(Expression<Func<ImageResponse, bool>> condition);
    Task<ImageResponse> GetImageByConditionAsync(Expression<Func<ImageResponse, bool>> condition);
    Task<ImageResponse> CreateImageAsync(ImageAddRequest imageRequest);
    Task<ImageResponse?> UpdateImageAsync(ImageUpdateRequest imageRequest);
    Task<bool> DeleteImageAsync(Guid id);

    Task<IEnumerable<ImageResponse>> GetImagesByPromptIdAsync(Guid promptId);
    Task<bool> AddImageToPromptAsync(Guid promptId, Guid imageId);
    Task<bool> RemoveImageFromPromptAsync(Guid promptId, Guid imageId);
}
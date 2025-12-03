using Promptino.Core.DTOs;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IImageGetterrService
{
    Task<IEnumerable<ImageResponse>> GetAllImagesAsync();
    Task<IEnumerable<ImageResponse>> GetImagesByConditionAsync(Expression<Func<ImageResponse, bool>> condition);
    Task<ImageResponse> GetImageByConditionAsync(Expression<Func<ImageResponse, bool>> condition);
    Task<IEnumerable<ImageResponse>> GetImagesByPromptIdAsync(Guid promptId);
}
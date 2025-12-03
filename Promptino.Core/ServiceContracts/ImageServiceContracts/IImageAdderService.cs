using Promptino.Core.DTOs;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IImageAdderService
{
    Task<ImageResponse> CreateImageAsync(ImageAddRequest imageRequest);
    Task<bool> AddImageToPromptAsync(Guid promptId, Guid imageId);
}
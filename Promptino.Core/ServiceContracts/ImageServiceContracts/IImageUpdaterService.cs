using Promptino.Core.DTOs;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IImageUpdaterService
{
    Task<ImageResponse?> UpdateImageAsync(ImageUpdateRequest imageRequest);
}
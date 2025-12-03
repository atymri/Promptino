using Promptino.Core.DTOs;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IImageDeleterService
{
    Task<bool> DeleteImageAsync(Guid id);
    Task<bool> RemoveImageFromPromptAsync(Guid promptId, Guid imageId);
}
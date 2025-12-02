using Promptino.Core.Domain.Entities;
using System.Linq.Expressions;

namespace Promptino.Core.Domain.RepositoryContracts;

public interface IImageRepository
{
    Task<IEnumerable<Image>> GetImagesAsync();
    Task<Image?> AddImageAsync(Image image);
    Task<Image?> UpdateImageAsync(Image image);
    Task<bool> DeleteImageAsync(Guid id);

    Task<IEnumerable<Image>> GetImagesByConditionAsync(Expression<Func<Image, bool>> condition);
    Task<Image?> GetImageByConditionAsync(Expression<Func<Image, bool>> condition);

    Task<IEnumerable<Image>> GetImagesByPromptIdAsync(Guid promptId);
    Task<bool> AddImageToPromptAsync(Guid promptId, Guid imageId);
    Task<bool> RemoveImageFromPromptAsync(Guid promptId, Guid imageId);
    bool DoesImageExist(Guid imageId);
}

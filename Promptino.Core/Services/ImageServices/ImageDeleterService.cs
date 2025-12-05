using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;

namespace Promptino.Core.Services.ImageServices;

public class ImageDeleterService : IImageDeleterService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IImageRepository _imageRepository;

    public ImageDeleterService(IImageRepository imageRepository,
        IPromptRepository promptRepository)
    {
        _imageRepository = imageRepository;
        _promptRepository = promptRepository;
    }

    public async Task<bool> DeleteImageAsync(Guid id)
    {
        if (!await _imageRepository.DoesImageExistAsync(id))
            throw new ImageNotFoundException("تصویر مورد نظر وجود ندارد");

        return await _imageRepository.DeleteImageAsync(id);
    }
    public async Task<bool> RemoveImageFromPromptAsync(Guid promptId, Guid imageId)
    {
        if (!await _promptRepository.DoesPromptExistAsync(promptId))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر یافت نشد");

        if (!await _imageRepository.DoesImageExistAsync(imageId))
            throw new ImageNotFoundException("تصویر مورد نظر یافت نشد");

        return await _imageRepository.RemoveImageFromPromptAsync(promptId, imageId);
    }

}
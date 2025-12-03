using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;
using System.Linq.Expressions;

namespace Promptino.Infrastructure.Repositories;

public class ImageRepositorry : IImageRepository
{
    private readonly ApplicationDbContext _context;
    public ImageRepositorry(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Image?> AddImageAsync(Image image)
    {
        await _context.Images.AddAsync(image);
        await _context.SaveChangesAsync();

        return image;
    }

    public async Task<bool> AddImageToPromptAsync(Guid promptId, Guid imageId)
    {
        var promptImage = new PromptImage()
        {
            ImageID = imageId,
            PromptID = promptId,
        };

        await _context.PromptImages.AddAsync(promptImage);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteImageAsync(Guid id)
    {
        var image = await _context.Images
            .Include(im => im.PromptImages)
            .FirstOrDefaultAsync(im => im.ID == id);

        if (image == null) return false;

        _context.Images.Remove(image);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DoesImageExistAsync(Guid imageId)
        => await _context.Images.AnyAsync(im => im.ID == imageId);

    public async Task<Image?> GetImageByConditionAsync(Expression<Func<Image, bool>> condition)
        => await _context.Images
        .Include(im => im.PromptImages)
        .ThenInclude(pi => pi.Prompt)
        .FirstOrDefaultAsync(condition);

    public async Task<IEnumerable<Image>> GetImagesAsync()
        => await _context.Images
        .Include(im => im.PromptImages)
        .ThenInclude(pi => pi.Prompt)
        .ToListAsync();

    public async Task<IEnumerable<Image>> GetImagesByConditionAsync(Expression<Func<Image, bool>> condition)
        => await _context.Images
        .Include(im => im.PromptImages)
        .ThenInclude(pi => pi.Prompt)
        .Where(condition)
        .ToListAsync();

    public async Task<IEnumerable<Image>> GetImagesByPromptIdAsync(Guid promptId)
     => await _context.PromptImages
        .Where(p => p.PromptID == promptId)
        .Select(im => im.Image)
        .ToListAsync();

    public async Task<bool> RemoveImageFromPromptAsync(Guid promptId, Guid imageId)
    {
        var promptImage = _context.PromptImages
            .FirstOrDefault(pi => pi.PromptID == promptId && pi.ImageID == imageId);

        if (promptImage == null) return false;

        _context.PromptImages.Remove(promptImage);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Image?> UpdateImageAsync(Image image)
    {
        if (image == null) return null;

        var saved = await _context.Images.FirstOrDefaultAsync(im => im.ID == image.ID);
        if (saved == null) return null;

        saved.Path = image.Path;
        saved.Title = image.Title;
        saved.GeneratedWith = image.GeneratedWith;
        saved.Touch();

        await _context.SaveChangesAsync();
        return saved;
    }
}

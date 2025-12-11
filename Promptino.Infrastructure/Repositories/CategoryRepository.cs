using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category> AddCategoryAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<bool> AddPromptToCategoryAsync(Guid promptId, Guid categoryId)
    {
        var promptCategory = new PromptCategories()
        {
            CategoryId = categoryId,
            PromptId = promptId,
        };

        await _context.PromptCategories.AddAsync(promptCategory);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteCategoryAsync(Guid categoryId)
    {
        var category = await _context.Categories
            .Include(c => c.PromptCategories)
            .SingleOrDefaultAsync(c => c.ID == categoryId);

        if (category is null) return false;

        if (category.PromptCategories.Any())
            _context.PromptCategories.RemoveRange(category.PromptCategories);

        _context.Categories.Remove(category);
        return await _context.SaveChangesAsync() > 0;

    }

    public async Task<bool> DeletePromptFromCategoryAsync(Guid promptId, Guid categoryId)
    {
        var promptCategory = await _context.PromptCategories
            .SingleOrDefaultAsync(pc => pc.PromptId == promptId && pc.CategoryId == categoryId);

        if (promptCategory is null) return false;

        _context.PromptCategories.Remove(promptCategory);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DoesCategoryExistAsync(Guid categoryId)
        => await _context.Categories.AnyAsync(c => c.ID == categoryId);

    public async Task<List<Category>> GetAllCategoriesAsync()
        => await _context.Categories
            .Include(c => c.PromptCategories)
            .ToListAsync();

    public async Task<List<Prompt>> GetCategoryPromptsAsync(string categoryName)
        => await _context.Prompts
        .Include(p => p.PromptImages)
        .ThenInclude(pi => pi.Image)
                .Where(p => p.PromptCategories.Any(pc => pc.Category.Title == categoryName))
        .ToListAsync();

    public async Task<bool> IsPromptInCategory(Guid categoryId, Guid promptId)
        => await _context.PromptCategories.AnyAsync(c => c.PromptId == promptId
        && c.CategoryId == categoryId);

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        var saved = await _context.Categories.FirstOrDefaultAsync(c => c.ID == category.ID);
        if (saved is null) return null;

        saved.Title = category.Title;
        saved.Description = category.Description;
        saved.Touch();

        await _context.SaveChangesAsync();

        return saved;
    }
}

using Promptino.Core.Domain.Entities;

namespace Promptino.Core.Domain.RerpositoryContracts;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category> AddCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<List<Prompt>> GetCategoryPromptsAsync(string categoryName);
    Task<bool> DeleteCategoryAsync(Guid categoryId);
    Task<bool> AddPromptToCategoryAsync(Guid promptId, Guid categoryId);
    Task<bool> DeletePromptFromCategoryAsync(Guid promptId, Guid categoryId);
    Task<bool> DoesCategoryExistAsync(Guid categoryId);
    Task<bool> IsPromptInCategory(Guid categoryId, Guid promptId);
}

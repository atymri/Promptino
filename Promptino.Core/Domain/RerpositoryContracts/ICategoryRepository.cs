using Promptino.Core.Domain.Entities;
using System.Linq.Expressions;

namespace Promptino.Core.Domain.RerpositoryContracts;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<Category>> GetCategoriesByCondition(Expression<Func<Category, bool>> condition);
    Task<Category?> GetCategoryByCondition(Expression<Func<Category, bool>> condition);
    Task<Category> AddCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<List<Prompt>> GetCategoryPromptsAsync(string categoryName);
    Task<bool> DeleteCategoryAsync(Guid categoryId);
    Task<bool> AddPromptToCategoryAsync(Guid promptId, Guid categoryId);
    Task<bool> DeletePromptFromCategoryAsync(Guid promptId, Guid categoryId);
    Task<bool> DoesCategoryExistAsync(string categoryTitle);
    Task<bool> DoesCategoryExistAsync(Guid categoryID);
    Task<bool> IsPromptInCategory(Guid categoryId, Guid promptId);
}

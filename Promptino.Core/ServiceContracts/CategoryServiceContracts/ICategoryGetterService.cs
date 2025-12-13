using Promptino.Core.DTOs;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.CategoryServiceContracts;

public interface ICategoryGetterService
{
    Task<List<CategoryResponse>> GetAllCategoriesAsync();
    Task<List<CategoryResponse>> SearchCategoriesAsync(string keyword);
    Task<List<CategoryResponse>> GetCategoriesByConditionAsync(Expression<Func<CategoryResponse, bool>> condition);
    Task<CategoryResponse> GetCategoryByConditionAsync(Expression<Func<CategoryResponse, bool>> condition);
    Task<List<PromptResponse>> GetPromptsFromCategoryAsync(string categoryName);
}

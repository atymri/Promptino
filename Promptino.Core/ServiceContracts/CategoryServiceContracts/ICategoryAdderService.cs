using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.CategoryServiceContracts;

public interface ICategoryAdderService
{
    Task<CategoryResponse> AddCategoryAsync(CategoryAddRequest request);
    Task<bool> AddPromptToCategory(AddPromptToCategoryRequest request);
}

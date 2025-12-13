using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.CategoryServiceContracts;

public interface ICategoryDeleterService
{
    Task<bool> DeleteCategory(Guid categoryID);
    Task<bool> RemovePromptFromCategorry(DeletePromptFromCategoryRequest request);
}


using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.CategoryServiceContracts;

public interface ICategoryUpdaterService
{
    Task<CategoryResponse> UpdateCategory(CategoryUpdateRequest request);
}

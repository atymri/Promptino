using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.CategoryServiceContracts;

namespace Promptino.Core.Services.CategoryServices;

public class CategoryDeleterService : ICategoryDeleterService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPromptRepository _promptRepository;
    public CategoryDeleterService(ICategoryRepository repository, IPromptRepository promptRepository)
    {
        _categoryRepository = repository;
        _promptRepository = promptRepository;
    }

    public async Task<bool> DeleteCategory(Guid categoryID)
    {
        var exists = await _categoryRepository.DoesCategoryExistAsync(categoryID);
        if (!exists)
            throw new CategoryNotFoundException("دسته بندی مورد نظر وجود ندارد");

        return await _categoryRepository.DeleteCategoryAsync(categoryID);
    }

    public async Task<bool> RemovePromptFromCategorry(DeletePromptFromCategoryRequest request)
    {
        if (request is null)
            throw new NullCategoryRequestException("درخواست نامعتبر");

        var prompt = await _promptRepository.DoesPromptExistAsync(request.PromptID);
        if (!prompt)
            throw new PromptNotFoundExceptions("پرامپت مورد نظر یافت نشد");

        var category = await _categoryRepository.DoesCategoryExistAsync(request.CategoryID);
        if (!category)
            throw new CategoryNotFoundException("دسته بندی مورد نظر یافت نشد");

        return await _categoryRepository.DeletePromptFromCategoryAsync(promptId: request.PromptID, categoryId:request.CategoryID);
    }
}

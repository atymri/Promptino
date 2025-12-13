using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.CategoryServiceContracts;

namespace Promptino.Core.Services.CategoryServices;

public class CategoryAdderService : ICategoryAdderService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPromptRepository _promptRepository;
    private readonly IMapper _mapper;
    public CategoryAdderService(ICategoryRepository categoryRepository, 
        IPromptRepository promptRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _promptRepository = promptRepository;
        _mapper = mapper;
    }

    public async Task<CategoryResponse> AddCategoryAsync(CategoryAddRequest request)
    {
        if (request is null)
            throw new NullCategoryRequestException(nameof(request));

        var exists = await _categoryRepository.DoesCategoryExistAsync(request.Title);
        if(exists)
            throw new CategoryExistsException(nameof(request));

        var category = _mapper.Map<Category>(request);
        var response = await _categoryRepository.AddCategoryAsync(category);

        return _mapper.Map<CategoryResponse>(response);
    }

    public async Task<bool> AddPromptToCategory(AddPromptToCategoryRequest request)
    {
        if (request is null)
            throw new NullCategoryRequestException(nameof(request));

        var prompt = await _promptRepository.DoesPromptExistAsync(request.PromptID);
        if (!prompt)
            throw new PromptNotFoundExceptions(nameof(prompt));

        var category = await _categoryRepository.DoesCategoryExistAsync(request.CategoryID);
        if (!category)
            throw new CategoryNotFoundException(nameof(category));

        var response = await _categoryRepository.AddPromptToCategoryAsync(request.PromptID, request.CategoryID);

        return response;
    }
}

using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.CategoryServiceContracts;

namespace Promptino.Core.Services.CategoryServices;

public class CategoryUpdaterService : ICategoryUpdaterService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    public CategoryUpdaterService(ICategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public async Task<CategoryResponse> UpdateCategory(CategoryUpdateRequest request)
    {
        if (request is null)
            throw new NullCategoryRequestException("درخواست نامعتبر");

        var exists = await _repository.DoesCategoryExistAsync(request.CategoryID);
        if(!exists)
            throw new CategoryNotFoundException("دسته بندی مورد نظر یافت نشد");

        var category = _mapper.Map<Category>(request);
        var response = await _repository.UpdateCategoryAsync(category);
        return _mapper.Map<CategoryResponse>(response);
    }
}

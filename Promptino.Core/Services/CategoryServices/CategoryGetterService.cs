
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.CategoryServiceContracts;
using System.Linq.Expressions;

namespace Promptino.Core.Services.CategoryServices;

public class CategoryGetterService : ICategoryGetterService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    public CategoryGetterService(ICategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        => _mapper.Map<List<CategoryResponse>>(await _repository.GetAllCategoriesAsync());

    public async Task<List<CategoryResponse>> GetCategoriesByConditionAsync(Expression<Func<CategoryResponse, bool>> condition)
    {
        var mappedCondition = _mapper.MapExpression<Expression<Func<Category, bool>>>(condition);
        return _mapper.Map<List<CategoryResponse>>(await _repository.GetCategoriesByCondition(mappedCondition));
    }

    public async Task<CategoryResponse> GetCategoryByConditionAsync(Expression<Func<CategoryResponse, bool>> condition)
    {
        var mappedCondition = _mapper.MapExpression<Expression<Func<Category, bool>>>(condition);
        return _mapper.Map<CategoryResponse>(await _repository.GetCategoryByCondition(mappedCondition));
    }

    public async Task<List<PromptResponse>> GetPromptsFromCategoryAsync(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
            throw new ArgumentNullException(nameof(categoryName));

        var res = await _repository.GetCategoryPromptsAsync(categoryName);
        return _mapper.Map<List<PromptResponse>>(res);
    }

    public async Task<List<CategoryResponse>> SearchCategoriesAsync(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            throw new ArgumentNullException(nameof(keyword));

        var categories = await _repository.GetCategoriesByCondition(c => c.Title == keyword);
        return _mapper.Map<List<CategoryResponse>>(categories);
    }
}

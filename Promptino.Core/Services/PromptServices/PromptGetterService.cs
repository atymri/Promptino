using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using System.Linq.Expressions;

namespace Promptino.Core.Services.PromptServices;

public class PromptGetterService : IPromptGetterService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IMapper _mapper;
    public PromptGetterService(IPromptRepository promptRepository, IMapper mapper)
    {
        _promptRepository = promptRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<PromptResponse>> GetAllPromptsAsync(int page = 1, int pageSize = PaginationDefaults.DefaultPageSize)
    {
        (page, pageSize) = Normalize(page, pageSize);
        var (totalCount, prompts) = await _promptRepository.GetPromptsPagedAsync(page, pageSize);
        return new PagedResult<PromptResponse>(_mapper.Map<List<PromptResponse>>(prompts), page, pageSize, totalCount);
    }

    private static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = PaginationDefaults.DefaultPageSize;
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;
        return (page, pageSize);
    }

    public async Task<IEnumerable<PromptResponse>> GetPromptsByOwnerAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        var prompts = await _promptRepository.GetPromptsByOwnerAsync(userId);
        return _mapper.Map<List<PromptResponse>>(prompts);
    }

    public async Task<PromptResponse> GetPromptByConditionAsync(Expression<Func<PromptResponse, bool>> condition)
    {
        if (condition is null)
            throw new ArgumentNullException(nameof(condition));

        var mappedCondition = _mapper.MapExpression<Expression<Func<Prompt, bool>>>(condition);

        return _mapper.Map<PromptResponse>
            (await _promptRepository.GetPromptByConditionAsync(mappedCondition));
    }

    public async Task<IEnumerable<PromptResponse>> GetPromptsByConditionAsync(Expression<Func<PromptResponse, bool>> condition)
    {
        if (condition is null)
            throw new ArgumentNullException("فیلتر نامعتبر ", nameof(condition));

        var mappedCondition = _mapper.MapExpression<Expression<Func<Prompt, bool>>>(condition);

        return _mapper.Map<List<PromptResponse>>
            (await _promptRepository.GetPromptsByConditionAsync(mappedCondition));
    }

    public async Task<PagedResult<PromptResponse>> SearchPromptsAsync(string keyword, int page = 1, int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if(string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("کلیدواژه نمیتواند خالی باشد", nameof(keyword));

        (page, pageSize) = Normalize(page, pageSize);
        var (totalCount, prompts) = await _promptRepository.SearchPromptPagedAsync(keyword, page, pageSize);
        return new PagedResult<PromptResponse>(_mapper.Map<List<PromptResponse>>(prompts), page, pageSize, totalCount);
    }
}

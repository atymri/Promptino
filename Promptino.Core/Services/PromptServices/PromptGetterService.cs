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

    public async Task<IEnumerable<PromptResponse>> GetAllPromptsAsync()
    //=>  _mapper.Map<List<PromptResponse>>(await _promptRepository.GetPromptsAsync()) 
    //?? new List<PromptResponse>();
    {
        var prompts = await _promptRepository.GetPromptsAsync();
        return _mapper.Map<List<PromptResponse>>(prompts);
    }

    public async Task<IEnumerable<FavoriteWithDetailsResponse>> GetFavoritePromptsAsync(Guid userId)
        => _mapper.Map<List<FavoriteWithDetailsResponse>>
             (await _promptRepository.GetFavoritePromptsAsync(userId));

    public async Task<FavoritePromptResponse> GetFavoritesAsync(Guid promptId)
    {
        var prompt = await _promptRepository.GetPromptByConditionAsync(p => p.ID == promptId);
        if (prompt == null)
            throw new PromptNotFoundExceptions("پرامپت مورد نظر وجود ندارد");

        var favs = await _promptRepository.GetFavoritesAsync(promptId);
        
        return _mapper.Map<FavoritePromptResponse>(favs);
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

    public async Task<bool> IsPromptFavoriteAsync(Guid userId, Guid promptId)
    {
        if(userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        if(promptId == Guid.Empty)
            throw new ArgumentException("آیدی پرامپت نمیتواند خالی باشد", nameof(promptId));

        return await _promptRepository.IsFavoriteAsync(promptId, userId);
    }

    public async Task<IEnumerable<PromptResponse>> SearchPromptsAsync(string keyword)
    {
        if(string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("کلیدواژه نمیتواند خالی باشد", nameof(keyword));
    
        var prompts = await _promptRepository.SearchPromptAsync(keyword);
        return _mapper.Map<List<PromptResponse>>(prompts);
    }
}
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptGetterService
{
    Task<IEnumerable<PromptResponse>> GetAllPromptsAsync();
    Task<IEnumerable<PromptResponse>> SearchPromptsAsync(string keyword);
    Task<IEnumerable<PromptResponse>> GetPromptsByConditionAsync(Expression<Func<PromptResponse, bool>> condition);
    Task<PromptResponse> GetPromptByConditionAsync(Expression<Func<PromptResponse, bool>> condition);
    Task<IEnumerable<FavoriteWithDetailsResponse>> GetFavoritePromptsAsync(Guid userId);
    Task<bool> IsPromptFavoriteAsync(Guid userId, Guid promptId);
}
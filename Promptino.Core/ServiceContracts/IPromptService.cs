using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts;

public interface IPromptService
{
    Task<IEnumerable<PromptResponse>> GetAllPromptsAsync();
    Task<IEnumerable<PromptResponse>> SearchPromptsAsync(string keyword);
    Task<IEnumerable<PromptResponse>> GetPromptsByConditionAsync(Expression<Func<PromptResponse, bool>> condition);
    Task<PromptResponse> GetPromptByConditionAsync(Expression<Func<PromptResponse, bool>> condition);
    Task<PromptResponse> CreatePromptAsync(PromptAddRequest promptRequest);
    Task<PromptResponse?> UpdatePromptAsync(PromptUpdateRequest promptRequest);
    Task<bool> DeletePromptAsync(Guid id);

    Task<IEnumerable<FavoriteWithDetailsResponse>> GetFavoritePromptsAsync(Guid userId);
    Task<FavoritePromptResponse> AddToFavoritesAsync(FavoritePromptAddRequest favoriteRequest);
    Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid promptId);
    Task<bool> IsPromptFavoriteAsync(Guid userId, Guid promptId);
}
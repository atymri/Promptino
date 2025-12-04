using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptAdderService
{
    Task<PromptResponse> CreatePromptAsync(PromptAddRequest promptRequest);
    Task<FavoritePromptResponse> AddToFavoritesAsync(FavoritePromptAddRequest favoriteRequest);
}
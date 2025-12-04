using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptDeleterService
{
    Task<bool> DeletePromptAsync(Guid id);
    Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid promptId);
}
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using System;
using System.Linq.Expressions;

namespace Promptino.Core.ServiceContracts.ImageServiceContracts;

public interface IPromptUpdaterService
{
    Task<PromptResponse?> UpdatePromptAsync(PromptUpdateRequest promptRequest);
}
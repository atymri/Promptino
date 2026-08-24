using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.SavedPromptServiceContracts;

namespace Promptino.Core.Services.SavedPromptServices;

public class SavedPromptAdderService : ISavedPromptAdderService
{
    private readonly IPromptRepository _promptRepository;
    private readonly ISavedPromptRepository _savedPromptRepository;
    private readonly IMapper _mapper;

    public SavedPromptAdderService(
        IPromptRepository promptRepository,
        ISavedPromptRepository savedPromptRepository,
        IMapper mapper)
    {
        _promptRepository = promptRepository;
        _savedPromptRepository = savedPromptRepository;
        _mapper = mapper;
    }

    public async Task<SavedWithDetailsResponse> SaveAsync(Guid userId, Guid promptId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(userId));

        if (!await _promptRepository.DoesPromptExistAsync(promptId))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر پیدا نشد");

        if (await _savedPromptRepository.IsSavedAsync(userId, promptId))
            throw new PromptExistsException("پرامپت مورد نظر در حال حاضر در ذخیره‌شده‌های شما وجود دارد");

        var saved = new SavedPrompt { UserID = userId, PromptID = promptId };
        var success = await _savedPromptRepository.AddSavedPromptAsync(saved);

        if (!success)
            throw new Exception("خطا در ذخیره پرامپت");

        saved.Prompt = await _promptRepository.GetPromptByConditionAsync(p => p.ID == promptId);
        return _mapper.Map<SavedWithDetailsResponse>(saved);
    }
}

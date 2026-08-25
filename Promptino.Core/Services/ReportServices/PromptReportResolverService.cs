using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ReportServiceContracts;

namespace Promptino.Core.Services.ReportServices;

public class PromptReportResolverService : IPromptReportResolverService
{
    private readonly IPromptReportRepository _reportRepository;
    private readonly IPromptRepository _promptRepository;
    private readonly ISavedPromptRepository _savedPromptRepository;

    public PromptReportResolverService(
        IPromptReportRepository reportRepository,
        IPromptRepository promptRepository,
        ISavedPromptRepository savedPromptRepository)
    {
        _reportRepository = reportRepository;
        _promptRepository = promptRepository;
        _savedPromptRepository = savedPromptRepository;
    }

    public async Task<bool> ResolveAsync(Guid reportId, Guid adminId, ModerationDecisionRequest decision)
    {
        if (adminId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(adminId));
        ArgumentNullException.ThrowIfNull(decision);

        var report = await _reportRepository.GetByIdAsync(reportId)
            ?? throw new CommentNotFoundException("گزارش مورد نظر پیدا نشد");

        if (report.Status != ReportStatus.Pending)
            throw new InvalidOperationException("این گزارش قبلاً بررسی شده است");

        if (decision.HidePrompt)
        {
            var prompt = await _promptRepository.GetPromptByConditionAsync(p => p.ID == report.PromptID);
            if (prompt is not null)
            {
                prompt.IsHidden = true;
                await _promptRepository.UpdateHiddenFlagAsync(prompt.ID, true);
            }

            // A hidden prompt should not keep occupying saved lists
            await _savedPromptRepository.RemoveAllForPromptAsync(report.PromptID);
        }

        report.Status = ReportStatus.Resolved;
        report.ResolvedByUserID = adminId;
        report.ResolvedAt = DateTime.UtcNow;

        return await _reportRepository.SaveAsync();
    }
}

using AutoMapper;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.ServiceContracts.ReportServiceContracts;

namespace Promptino.Core.Services.ReportServices;

public class PromptReportAdderService : IPromptReportAdderService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IPromptReportRepository _reportRepository;

    public PromptReportAdderService(
        IPromptRepository promptRepository,
        IPromptReportRepository reportRepository)
    {
        _promptRepository = promptRepository;
        _reportRepository = reportRepository;
    }

    public async Task<PromptReportResponse> AddReportAsync(Guid reporterId, PromptReportAddRequest request)
    {
        if (reporterId == Guid.Empty)
            throw new ArgumentException("آیدی کاربر نمیتواند خالی باشد", nameof(reporterId));
        ArgumentNullException.ThrowIfNull(request);

        if (!await _promptRepository.DoesPromptExistAsync(request.PromptID))
            throw new PromptNotFoundExceptions("پرامپت مورد نظر پیدا نشد");

        if (await _reportRepository.HasPendingReportAsync(reporterId, request.PromptID))
            throw new PromptExistsException("شما قبلاً این پرامپت را گزارش داده‌اید و گزارش در انتظار بررسی است");

        var report = new PromptReport
        {
            ReporterID = reporterId,
            PromptID = request.PromptID,
            Reason = request.Reason,
            Status = ReportStatus.Pending
        };

        var saved = await _reportRepository.AddAsync(report);
        return ToResponse(saved);
    }

    internal static PromptReportResponse ToResponse(PromptReport r)
        => new(
            r.ID,
            r.ReporterID,
            r.Reporter?.UserName ?? string.Empty,
            r.PromptID,
            r.Prompt?.Title ?? string.Empty,
            r.Reason,
            r.Status.ToString(),
            r.CreatedAt);
}

using AutoMapper;
using Moq;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.Services.ReportServices;

namespace Promptino.Tests.ServiceTests;

public class PromptReportServiceTest
{
    private readonly Mock<IPromptRepository> _promptRepo = new();
    private readonly Mock<IPromptReportRepository> _reportRepo = new();

    private PromptReportAdderService MakeAdder()
        => new(_promptRepo.Object, _reportRepo.Object);

    [Fact]
    public async Task AddReportAsync_ShouldThrow_WhenPromptNotFound()
    {
        _promptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            MakeAdder().AddReportAsync(Guid.NewGuid(), new PromptReportAddRequest(Guid.NewGuid(), "spam content")));
    }

    [Fact]
    public async Task AddReportAsync_ShouldThrow_WhenAlreadyPending()
    {
        _promptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        _reportRepo.Setup(r => r.HasPendingReportAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<PromptExistsException>(() =>
            MakeAdder().AddReportAsync(Guid.NewGuid(), new PromptReportAddRequest(Guid.NewGuid(), "spam content")));
    }

    [Fact]
    public async Task AddReportAsync_ShouldSavePendingReport()
    {
        var promptId = Guid.NewGuid();
        PromptReport? saved = null;

        _promptRepo.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);
        _reportRepo.Setup(r => r.HasPendingReportAsync(It.IsAny<Guid>(), promptId)).ReturnsAsync(false);
        _reportRepo.Setup(r => r.AddAsync(It.IsAny<PromptReport>()))
            .Callback<PromptReport>(r => saved = r)
            .ReturnsAsync((PromptReport r) => r);

        var result = await MakeAdder().AddReportAsync(Guid.NewGuid(), new PromptReportAddRequest(promptId, "spam content"));

        Assert.Equal(promptId, result.PromptId);
        Assert.Equal("Pending", result.Status);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task ResolveAsync_ShouldHidePrompt_AndResolve()
    {
        var reportId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var report = new PromptReport { ID = reportId, PromptID = promptId, Reason = "bad", Status = ReportStatus.Pending };
        var prompt = new Prompt { ID = promptId, UserID = Guid.NewGuid(), Title = "t", Description = "d", Content = "c" };

        _reportRepo.Setup(r => r.GetByIdAsync(reportId)).ReturnsAsync(report);
        _promptRepo.Setup(r => r.GetPromptByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Prompt, bool>>>()))
            .ReturnsAsync(prompt);
        _reportRepo.Setup(r => r.SaveAsync()).ReturnsAsync(true);

        var resolver = new PromptReportResolverService(
            _reportRepo.Object, _promptRepo.Object, new Mock<ISavedPromptRepository>().Object);

        var result = await resolver.ResolveAsync(reportId, adminId, new ModerationDecisionRequest(HidePrompt: true));

        Assert.True(result);
        _promptRepo.Verify(r => r.UpdateHiddenFlagAsync(promptId, true), Times.Once);
        Assert.Equal(ReportStatus.Resolved, report.Status);
        Assert.Equal(adminId, report.ResolvedByUserID);
        Assert.NotNull(report.ResolvedAt);
    }

    [Fact]
    public async Task ResolveAsync_ShouldNotTouchPrompt_WhenDismissed()
    {
        var reportId = Guid.NewGuid();
        var report = new PromptReport { ID = reportId, PromptID = Guid.NewGuid(), Reason = "bad", Status = ReportStatus.Pending };

        _reportRepo.Setup(r => r.GetByIdAsync(reportId)).ReturnsAsync(report);
        _reportRepo.Setup(r => r.SaveAsync()).ReturnsAsync(true);

        var resolver = new PromptReportResolverService(
            _reportRepo.Object, _promptRepo.Object, new Mock<ISavedPromptRepository>().Object);

        var result = await resolver.ResolveAsync(reportId, Guid.NewGuid(), new ModerationDecisionRequest(HidePrompt: false));

        Assert.True(result);
        _promptRepo.Verify(r => r.UpdateHiddenFlagAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        Assert.Equal(ReportStatus.Resolved, report.Status);
    }

    [Fact]
    public async Task ResolveAsync_ShouldThrow_WhenAlreadyResolved()
    {
        var report = new PromptReport { ID = Guid.NewGuid(), PromptID = Guid.NewGuid(), Status = ReportStatus.Dismissed };
        _reportRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(report);

        var resolver = new PromptReportResolverService(
            _reportRepo.Object, _promptRepo.Object, new Mock<ISavedPromptRepository>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(report.ID, Guid.NewGuid(), new ModerationDecisionRequest(true)));
    }
}

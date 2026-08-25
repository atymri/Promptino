using Promptino.Core.DTOs;

namespace Promptino.Core.ServiceContracts.ReportServiceContracts;

public interface IPromptReportResolverService
{
    // Resolves a report: optionally hides the reported prompt (soft-hide), marks report resolved/dismissed
    Task<bool> ResolveAsync(Guid reportId, Guid adminId, ModerationDecisionRequest decision);
}

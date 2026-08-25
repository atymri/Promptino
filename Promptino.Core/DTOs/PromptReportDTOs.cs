namespace Promptino.Core.DTOs;

public record PromptReportAddRequest(
    Guid PromptID,
    string Reason
)
{
    public PromptReportAddRequest() : this(default, default)
    { }
};

public record PromptReportResponse(
    Guid Id,
    Guid ReporterId,
    string ReporterName,
    Guid PromptId,
    string PromptTitle,
    string Reason,
    string Status,
    DateTime CreatedAt
)
{
    public PromptReportResponse() : this(default, default, default, default, default, default, default, default)
    { }
};

public record ModerationDecisionRequest(
    bool HidePrompt
)
{
    public ModerationDecisionRequest() : this(true)
    { }
};

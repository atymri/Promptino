using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public enum ReportStatus
{
    Pending = 0,
    Resolved = 1,
    Dismissed = 2
}

public class PromptReport : BaseEntity<Guid>
{
    [ForeignKey(nameof(Reporter))]
    public Guid ReporterID { get; set; }
    [ForeignKey(nameof(Prompt))]
    public Guid PromptID { get; set; }

    public string Reason { get; set; } = string.Empty;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Guid? ResolvedByUserID { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public ApplicationUser Reporter { get; set; } = null!;
    public Prompt Prompt { get; set; } = null!;
}

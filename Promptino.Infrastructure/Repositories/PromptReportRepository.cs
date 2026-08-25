using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure.Repositories;

public class PromptReportRepository : IPromptReportRepository
{
    private readonly ApplicationDbContext _context;

    public PromptReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PromptReport?> GetByIdAsync(Guid reportId)
        => _context.PromptReports
            .Include(r => r.Reporter)
            .Include(r => r.Prompt)
            .FirstOrDefaultAsync(r => r.ID == reportId);

    public Task<bool> HasPendingReportAsync(Guid reporterId, Guid promptId)
        => _context.PromptReports.AnyAsync(r =>
            r.ReporterID == reporterId && r.PromptID == promptId && r.Status == ReportStatus.Pending);

    public async Task<PromptReport> AddAsync(PromptReport report)
    {
        await _context.PromptReports.AddAsync(report);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(report.ID) ?? report;
    }

    public async Task<(int TotalCount, IReadOnlyList<PromptReport> Items)> GetPendingPagedAsync(int page, int pageSize)
    {
        var query = _context.PromptReports
            .Where(r => r.Status == ReportStatus.Pending);

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(r => r.Reporter)
            .Include(r => r.Prompt)
            .OrderBy(r => r.CreatedAt) // oldest first: queue drains in complaint order
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (totalCount, items);
    }

    public async Task<bool> SaveAsync()
        => await _context.SaveChangesAsync() > 0;
}

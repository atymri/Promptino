using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure.Repositories;

public class PromptVersionRepository : IPromptVersionRepository
{
    private readonly ApplicationDbContext _context;

    public PromptVersionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextVersionNumberAsync(Guid promptId)
    {
        var max = await _context.PromptVersions
            .Where(v => v.PromptID == promptId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync();
        return (max ?? 0) + 1;
    }

    public async Task AddAsync(PromptVersion version)
    {
        await _context.PromptVersions.AddAsync(version);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PromptVersion>> GetForPromptAsync(Guid promptId)
        => await _context.PromptVersions
            .AsNoTracking()
            .Where(v => v.PromptID == promptId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

    public Task<PromptVersion?> GetAsync(Guid promptId, int versionNumber)
        => _context.PromptVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.PromptID == promptId && v.VersionNumber == versionNumber);
}

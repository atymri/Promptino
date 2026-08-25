using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure.Repositories;

public class SavedPromptRepository : ISavedPromptRepository
{
    private readonly ApplicationDbContext _context;

    public SavedPromptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SavedPrompt>> GetSavedByUserAsync(Guid userId)
        => await _context.SavedPrompts
            .Where(s => s.UserID == userId)
            .Include(s => s.Prompt)
            .ThenInclude(p => p.PromptImages)
            .ThenInclude(pi => pi.Image)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<bool> IsSavedAsync(Guid userId, Guid promptId)
        => await _context.SavedPrompts
            .AnyAsync(s => s.UserID == userId && s.PromptID == promptId);

    public async Task<bool> AddSavedPromptAsync(SavedPrompt saved)
    {
        await _context.SavedPrompts.AddAsync(saved);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveSavedPromptAsync(Guid userId, Guid promptId)
    {
        var saved = await _context.SavedPrompts
            .FirstOrDefaultAsync(s => s.UserID == userId && s.PromptID == promptId);

        if (saved == null) return false;

        _context.SavedPrompts.Remove(saved);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetSavedCountAsync(Guid promptId)
        => await _context.SavedPrompts.CountAsync(s => s.PromptID == promptId);

    public async Task RemoveAllForPromptAsync(Guid promptId)
        => await _context.SavedPrompts
            .Where(s => s.PromptID == promptId)
            .ExecuteDeleteAsync();
}

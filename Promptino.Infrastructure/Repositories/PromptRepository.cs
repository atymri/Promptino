using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;
using System.Linq.Expressions;

namespace Promptino.Infrastructure.Repositories;

public class PromptRepository : IPromptRepository
{
    private readonly ApplicationDbContext _context;

    public PromptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Prompt?> AddPromptAsync(Prompt prompt)
    {
        await _context.Prompts.AddAsync(prompt);
        await _context.SaveChangesAsync();

        return prompt;
    }

    public async Task<bool> DeletePromptAsync(Guid id)
    {
        var prompt = await _context.Prompts
            .Include(p => p.PromptImages)
            .Include(p => p.PromptCategories)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Likes)
            .Include(p => p.Reactions)
            .Include(p => p.SavedPrompts)
            .SingleOrDefaultAsync(p => p.ID == id);

        if (prompt == null) return false;

        if (prompt.PromptImages.Any())
            _context.PromptImages.RemoveRange(prompt.PromptImages);

        if (prompt.PromptCategories.Any())
            _context.PromptCategories.RemoveRange(prompt.PromptCategories);

        // EF InMemory does not enforce DB-level cascades
        if (prompt.Comments.Any())
        {
            _context.CommentLikes.RemoveRange(prompt.Comments.SelectMany(c => c.Likes));
            _context.Comments.RemoveRange(prompt.Comments);
        }

        if (prompt.Reactions.Any())
            _context.PromptReactions.RemoveRange(prompt.Reactions);

        if (prompt.SavedPrompts.Any())
            _context.SavedPrompts.RemoveRange(prompt.SavedPrompts);

        _context.Prompts.Remove(prompt);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DoesPromptExistAsync(Guid promptId)
        => await _context.Prompts.FindAsync(promptId) != null;

    public async Task<Guid?> GetPromptOwnerIdAsync(Guid promptId)
        => await _context.Prompts
            .Where(p => p.ID == promptId)
            .Select(p => (Guid?)p.UserID)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Prompt>> GetPromptsByOwnerAsync(Guid userId)
        => await _context.Prompts
            .Include(p => p.PromptCategories)
            .ThenInclude(p => p.Category)
            .Include(p => p.PromptImages)
            .ThenInclude(pi => pi.Image)
            .Where(p => p.UserID == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Prompt?> GetPromptByConditionAsync(Expression<Func<Prompt, bool>> condition)
        => await _context.Prompts
            .Include(p => p.User)
            .Include(p => p.PromptCategories)
            .ThenInclude(p => p.Category)
            .Include(p => p.PromptImages)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .Include(p => p.Reactions)
            .Include(p => p.SavedPrompts)
            .FirstOrDefaultAsync(condition);

    public async Task<(int TotalCount, IReadOnlyList<Prompt> Items)> GetPromptsPagedAsync(int page, int pageSize)
    {
        var query = _context.Prompts
            .AsNoTracking();

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(p => p.User)
            .Include(p => p.PromptCategories)
            .ThenInclude(p => p.Category)
            .Include(p => p.PromptImages)
            .ThenInclude(pi => pi.Image)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (totalCount, items);
    }

    public async Task<IEnumerable<Prompt>> GetPromptsByConditionAsync(Expression<Func<Prompt, bool>> condition)
        => await _context.Prompts
        .Include(p => p.User)
        .Include(p => p.PromptCategories)
        .ThenInclude(p => p.Category)
        .Include(p => p.PromptImages)
        .ThenInclude(pi => pi.Image)
        .Include(p => p.Comments)
        .ThenInclude(c => c.User)
        .Include(p => p.Reactions)
        .Include(p => p.SavedPrompts)
        .Where(condition)
        .ToListAsync();

    public async Task<(int TotalCount, IReadOnlyList<Prompt> Items)> SearchPromptPagedAsync(string keyword, int page, int pageSize)
    {
        if (keyword == null) return (0, Array.Empty<Prompt>());

        var query = _context.Prompts
            .AsNoTracking()
            .Where(p => p.Title.ToLower().Contains(keyword.ToLower())
                     || p.Description.ToLower().Contains(keyword.ToLower()));

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(p => p.User)
            .Include(p => p.PromptImages)
            .ThenInclude(pi => pi.Image)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (totalCount, items);
    }

    public async Task<Prompt?> UpdatePromptAsync(Prompt prompt)
    {
        var existingPrompt = await _context.Prompts
            .Include(p => p.PromptImages)
            .FirstOrDefaultAsync(p => p.ID == prompt.ID);

        if (existingPrompt == null) return null;

        existingPrompt.Title = prompt.Title;
        existingPrompt.Description = prompt.Description;
        existingPrompt.Content = prompt.Content;

        existingPrompt.Touch(); // sets the last updated attr.

        await _context.SaveChangesAsync();
        return existingPrompt;
    }
}

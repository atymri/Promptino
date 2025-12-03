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

    public async Task<bool> AddToFavoritesAsync(Guid userId, Guid promptId)
    {
        await _context.FavoritePrompts.AddAsync(new FavoritePrompts
        {
            UserID = userId,
            PromptID = promptId,
        });

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeletePromptAsync(Guid id)
    {
        var prompt = await _context.Prompts.FindAsync(id);
        if (prompt == null) return false;

        _context.Prompts.Remove(prompt);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DoesPromptExistAsync(Guid promptId)
        => await _context.Prompts.FindAsync(promptId) != null;

    public async Task<IEnumerable<Prompt>> GetFavoritePromptsAsync(Guid userId)
    => await _context.Prompts
        .Where(p => p.FavoritePrompts.Any(fp => fp.UserID == userId))
        .Include(p => p.PromptImages)
        .ThenInclude(pi => pi.Image)
        .ToListAsync();

    public async Task<Prompt?> GetPromptByConditionAsync(
        Expression<Func<Prompt, bool>> condition)
        => await _context.Prompts
            .Include(p => p.PromptImages)
            .ThenInclude(pi => pi.Image)
            .FirstOrDefaultAsync(condition);

    public async Task<IEnumerable<Prompt>> GetPromptsAsync()
        => await _context.Prompts
        .Include(p => p.PromptImages)
        .ThenInclude(pi => pi.Image)
        .ToListAsync();

    public async Task<IEnumerable<Prompt>> GetPromptsByConditionAsync(Expression<Func<Prompt, bool>> condition)
        => await _context.Prompts
        .Include(p => p.PromptImages)
        .ThenInclude(pi => pi.Image)
        .Where(condition)
        .ToListAsync();

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid promptId)
        => await _context.FavoritePrompts
            .AnyAsync(fp => fp.UserID == userId && fp.PromptID == promptId);

    public async Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid promptId)
    {
        var favorite = await _context.FavoritePrompts
            .FirstOrDefaultAsync(fp => fp.UserID == userId && fp.PromptID == promptId);

        if (favorite == null) return false;

        _context.FavoritePrompts.Remove(favorite);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Prompt>> SearchPromptAsync(string keyword)
    {
        if (keyword == null) return null;

        return await _context.Prompts
            .Include(p => p.PromptImages)
            .ThenInclude(pi => pi.Image)
            .Where(p => p.Title.ToLower().Contains(keyword.ToLower())
                     || p.Description.ToLower().Contains(keyword.ToLower()))
            .ToListAsync();
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
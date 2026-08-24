using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure.Repositories;

public class PromptReactionRepository : IPromptReactionRepository
{
    private readonly ApplicationDbContext _context;

    public PromptReactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PromptReaction?> GetReactionAsync(Guid userId, Guid promptId)
        => await _context.PromptReactions
            .FirstOrDefaultAsync(r => r.UserID == userId && r.PromptID == promptId);

    public async Task AddReactionAsync(PromptReaction reaction)
    {
        await _context.PromptReactions.AddAsync(reaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateReactionAsync(PromptReaction reaction)
    {
        _context.PromptReactions.Update(reaction);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> RemoveReactionAsync(Guid userId, Guid promptId)
    {
        var reaction = await GetReactionAsync(userId, promptId);
        if (reaction == null) return false;

        _context.PromptReactions.Remove(reaction);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<(int Likes, int Dislikes)> GetCountsAsync(Guid promptId)
    {
        var likes = await _context.PromptReactions
            .CountAsync(r => r.PromptID == promptId && r.Type == ReactionType.Like);
        var dislikes = await _context.PromptReactions
            .CountAsync(r => r.PromptID == promptId && r.Type == ReactionType.Dislike);

        return (likes, dislikes);
    }
}

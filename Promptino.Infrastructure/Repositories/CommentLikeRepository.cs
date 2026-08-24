using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure.Repositories;

public class CommentLikeRepository : ICommentLikeRepository
{
    private readonly ApplicationDbContext _context;

    public CommentLikeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<CommentLike?> GetLikeAsync(Guid userId, Guid commentId)
        => _context.CommentLikes
            .FirstOrDefaultAsync(l => l.UserID == userId && l.CommentID == commentId);

    public async Task AddLikeAsync(CommentLike like)
    {
        await _context.CommentLikes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> RemoveLikeAsync(Guid userId, Guid commentId)
    {
        var like = await GetLikeAsync(userId, commentId);
        if (like == null) return false;

        _context.CommentLikes.Remove(like);
        return await _context.SaveChangesAsync() > 0;
    }

    public Task<int> GetCountAsync(Guid commentId)
        => _context.CommentLikes.CountAsync(l => l.CommentID == commentId);
}

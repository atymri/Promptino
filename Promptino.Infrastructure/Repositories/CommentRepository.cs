using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;

    public CommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Roots only; the service loads replies per root so paging stays bounded
    public async Task<(int TotalCount, IReadOnlyList<Comment> Items)> GetRootsByPromptPagedAsync(Guid promptId, int page, int pageSize)
    {
        var query = _context.Comments
            .Where(c => c.PromptID == promptId && c.ParentCommentID == null);

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(c => c.User)
            .Include(c => c.Likes)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (totalCount, items);
    }

    public async Task<IReadOnlyList<Comment>> GetRepliesForRootsAsync(IReadOnlyList<Guid> rootIds)
        => await _context.Comments
            .Where(c => c.ParentCommentID != null && rootIds.Contains(c.ParentCommentID.Value))
            .Include(c => c.User)
            .Include(c => c.Likes)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Comment?> GetByIdAsync(Guid commentId)
        => await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.ID == commentId);

    public async Task<Comment> AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);

        // re-read so the User navigation is populated for mapping the response
        await _context.SaveChangesAsync();
        return await GetByIdAsync(comment.ID) ?? comment;
    }

    public async Task<bool> DeleteAsync(Guid commentId)
    {
        var comment = await _context.Comments
            .Include(c => c.Replies)
            .ThenInclude(r => r.Likes)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.ID == commentId);

        if (comment == null) return false;

        // EF InMemory does not enforce DB cascades; the self-FK is Restrict on SQL Server too
        _context.CommentLikes.RemoveRange(
            comment.Likes.Concat(comment.Replies.SelectMany(r => r.Likes)));

        if (comment.Replies.Any())
            _context.Comments.RemoveRange(comment.Replies);

        _context.Comments.Remove(comment);
        return await _context.SaveChangesAsync() > 0;
    }
}

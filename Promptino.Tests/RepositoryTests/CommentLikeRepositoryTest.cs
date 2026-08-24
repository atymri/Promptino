using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Infrastructure.DatabaseContext;
using Promptino.Infrastructure.Repositories;

namespace Promptino.Infrastructure.Tests.Repositories;

public class CommentLikeRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CommentLikeRepository _repository;
    private readonly ApplicationUser _user;
    private readonly Comment _comment;

    public CommentLikeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CommentLikeRepository(_context);

        _user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u@example.com", Email = "u@example.com" };
        var prompt = new Prompt { ID = Guid.NewGuid(), UserID = _user.Id, Title = "P", Description = "d", Content = "c" };
        _comment = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = prompt.ID, Content = "hello" };

        _context.Users.Add(_user);
        _context.Prompts.Add(prompt);
        _context.Comments.Add(_comment);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddLikeAsync_ThenGetLike_ReturnsIt()
    {
        await _repository.AddLikeAsync(new CommentLike { UserID = _user.Id, CommentID = _comment.ID });

        var like = await _repository.GetLikeAsync(_user.Id, _comment.ID);

        Assert.NotNull(like);
        Assert.Equal(_user.Id, like.UserID);
    }

    [Fact]
    public async Task GetLikeAsync_ReturnsNull_WhenNotLiked()
    {
        var like = await _repository.GetLikeAsync(Guid.NewGuid(), _comment.ID);

        Assert.Null(like);
    }

    [Fact]
    public async Task RemoveLikeAsync_RemovesAndReturnsTrue_WhenExists()
    {
        await _repository.AddLikeAsync(new CommentLike { UserID = _user.Id, CommentID = _comment.ID });

        var result = await _repository.RemoveLikeAsync(_user.Id, _comment.ID);

        Assert.True(result);
        Assert.Null(await _repository.GetLikeAsync(_user.Id, _comment.ID));
    }

    [Fact]
    public async Task RemoveLikeAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.RemoveLikeAsync(_user.Id, _comment.ID);

        Assert.False(result);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsLikesForSpecificComment()
    {
        var otherUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "o@example.com", Email = "o@example.com" };
        _context.Users.Add(otherUser);

        await _repository.AddLikeAsync(new CommentLike { UserID = _user.Id, CommentID = _comment.ID });
        await _repository.AddLikeAsync(new CommentLike { UserID = otherUser.Id, CommentID = _comment.ID });

        var otherComment = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _comment.PromptID, Content = "other" };
        _context.Comments.Add(otherComment);
        await _context.SaveChangesAsync();

        await _repository.AddLikeAsync(new CommentLike { UserID = _user.Id, CommentID = otherComment.ID });

        var count = await _repository.GetCountAsync(_comment.ID);

        Assert.Equal(2, count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

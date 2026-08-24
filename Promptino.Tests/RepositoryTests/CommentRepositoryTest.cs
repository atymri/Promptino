using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using Promptino.Infrastructure.DatabaseContext;
using Promptino.Infrastructure.Repositories;

namespace Promptino.Infrastructure.Tests.Repositories;

public class CommentRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CommentRepository _repository;
    private readonly ApplicationUser _user;
    private readonly Prompt _prompt;

    public CommentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CommentRepository(_context);

        _user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "cu@example.com", Email = "cu@example.com", FirstName = "C", LastName = "U" };
        _prompt = new Prompt { ID = Guid.NewGuid(), UserID = _user.Id, Title = "P", Description = "d", Content = "c" };

        _context.Users.Add(_user);
        _context.Prompts.Add(_prompt);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_SavesAndReturnsComment_WithUserIncluded()
    {
        var comment = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, Content = "hello" };

        var result = await _repository.AddAsync(comment);

        Assert.NotNull(result);
        Assert.Equal(comment.ID, result.ID);
        Assert.NotNull(result.User);
        Assert.Equal(_user.Id, result.User.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCommentWithUser()
    {
        var comment = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, Content = "hello" };
        await _repository.AddAsync(comment);

        var result = await _repository.GetByIdAsync(comment.ID);

        Assert.NotNull(result);
        Assert.Equal("hello", result.Content);
        Assert.NotNull(result.User);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPromptAsync_ReturnsCommentsForThatPromptOnly_AndIncludesLikes()
    {
        var otherPrompt = new Prompt { ID = Guid.NewGuid(), UserID = _user.Id, Title = "Q", Description = "d", Content = "c" };
        _context.Prompts.Add(otherPrompt);

        var c1 = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, Content = "a" };
        c1.Likes.Add(new CommentLike { UserID = Guid.NewGuid(), CommentID = c1.ID });

        await _repository.AddAsync(c1);
        await _repository.AddAsync(new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, Content = "b" });
        await _repository.AddAsync(new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = otherPrompt.ID, Content = "z" });
        await _context.SaveChangesAsync();

        var result = (await _repository.GetRootsByPromptPagedAsync(_prompt.ID, 1, PaginationDefaults.MaxPageSize)).Items;

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.NotEqual("z", c.Content));

        var first = result.Single(c => c.Content == "a");
        Assert.NotNull(first.Likes);
        Assert.Single(first.Likes);
    }

    [Fact]
    public async Task DeleteAsync_RootRemovesRepliesAndLikes()
    {
        var root = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, Content = "root" };
        var reply = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, ParentCommentID = root.ID, Content = "reply" };
        root.Likes.Add(new CommentLike { UserID = Guid.NewGuid(), CommentID = root.ID });
        reply.Likes.Add(new CommentLike { UserID = Guid.NewGuid(), CommentID = reply.ID });

        await _repository.AddAsync(root);
        await _repository.AddAsync(reply);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(root.ID);

        Assert.True(result);
        Assert.Null(await _repository.GetByIdAsync(root.ID));
        Assert.Null(await _repository.GetByIdAsync(reply.ID));
        Assert.Empty(await _context.CommentLikes.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReplyLeavesRootIntact()
    {
        var root = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, Content = "root" };
        var reply = new Comment { ID = Guid.NewGuid(), UserID = _user.Id, PromptID = _prompt.ID, ParentCommentID = root.ID, Content = "reply" };

        await _repository.AddAsync(root);
        await _repository.AddAsync(reply);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(reply.ID);

        Assert.True(result);
        Assert.Null(await _repository.GetByIdAsync(reply.ID));
        Assert.NotNull(await _repository.GetByIdAsync(root.ID));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

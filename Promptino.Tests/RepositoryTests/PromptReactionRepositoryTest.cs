using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Infrastructure.DatabaseContext;
using Promptino.Infrastructure.Repositories;

namespace Promptino.Infrastructure.Tests.Repositories;

public class PromptReactionRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PromptReactionRepository _repository;
    private readonly ApplicationUser _user1;
    private readonly ApplicationUser _user2;
    private readonly Prompt _prompt;

    public PromptReactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PromptReactionRepository(_context);

        _user1 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u1@example.com", Email = "u1@example.com" };
        _user2 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u2@example.com", Email = "u2@example.com" };
        _prompt = new Prompt
        {
            ID = Guid.NewGuid(),
            UserID = _user1.Id,
            Title = "P",
            Description = "d",
            Content = "c"
        };

        _context.Users.AddRange(_user1, _user2);
        _context.Prompts.Add(_prompt);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddReactionAsync_ThenGetReaction_ReturnsIt()
    {
        await _repository.AddReactionAsync(new PromptReaction { UserID = _user1.Id, PromptID = _prompt.ID, Type = ReactionType.Like });

        var reaction = await _repository.GetReactionAsync(_user1.Id, _prompt.ID);

        Assert.NotNull(reaction);
        Assert.Equal(ReactionType.Like, reaction.Type);
    }

    [Fact]
    public async Task UpdateReactionAsync_ChangesType()
    {
        await _repository.AddReactionAsync(new PromptReaction { UserID = _user1.Id, PromptID = _prompt.ID, Type = ReactionType.Like });

        var reaction = await _repository.GetReactionAsync(_user1.Id, _prompt.ID);
        reaction.Type = ReactionType.Dislike;
        reaction.Touch();
        await _repository.UpdateReactionAsync(reaction);

        var updated = await _repository.GetReactionAsync(_user1.Id, _prompt.ID);
        Assert.Equal(ReactionType.Dislike, updated.Type);
    }

    [Fact]
    public async Task RemoveReactionAsync_RemovesAndReturnsTrue_WhenExists()
    {
        await _repository.AddReactionAsync(new PromptReaction { UserID = _user1.Id, PromptID = _prompt.ID, Type = ReactionType.Like });

        var result = await _repository.RemoveReactionAsync(_user1.Id, _prompt.ID);

        Assert.True(result);
        Assert.Null(await _repository.GetReactionAsync(_user1.Id, _prompt.ID));
    }

    [Fact]
    public async Task RemoveReactionAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.RemoveReactionAsync(_user1.Id, _prompt.ID);

        Assert.False(result);
    }

    [Fact]
    public async Task GetCountsAsync_CountsLikesAndDislikesSeparately()
    {
        await _repository.AddReactionAsync(new PromptReaction { UserID = _user1.Id, PromptID = _prompt.ID, Type = ReactionType.Like });
        await _repository.AddReactionAsync(new PromptReaction { UserID = _user2.Id, PromptID = _prompt.ID, Type = ReactionType.Like });

        var otherPrompt = new Prompt { ID = Guid.NewGuid(), UserID = _user1.Id, Title = "Q", Description = "d", Content = "c" };
        _context.Prompts.Add(otherPrompt);
        await _context.SaveChangesAsync();
        await _repository.AddReactionAsync(new PromptReaction { UserID = _user2.Id, PromptID = otherPrompt.ID, Type = ReactionType.Dislike });

        var (likes, dislikes) = await _repository.GetCountsAsync(_prompt.ID);

        Assert.Equal(2, likes);
        Assert.Equal(0, dislikes);
    }

    [Fact]
    public async Task GetCountsAsync_EmptyPrompt_ReturnsZeros()
    {
        var (likes, dislikes) = await _repository.GetCountsAsync(Guid.NewGuid());

        Assert.Equal(0, likes);
        Assert.Equal(0, dislikes);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

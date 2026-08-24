using AutoMapper;
using Moq;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.Services.PromptReactionServices;

namespace Promptino.Tests.ServiceTests;

public class PromptReactionServiceTest
{
    private readonly IMapper _mapper;

    public PromptReactionServiceTest()
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    private static (Mock<IPromptRepository>, Mock<IPromptReactionRepository>) MakeRepos(
        bool promptExists = true, PromptReaction? existing = null)
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(promptExists);

        var mockReactionRepo = new Mock<IPromptReactionRepository>();
        mockReactionRepo.Setup(r => r.GetReactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(existing);
        mockReactionRepo.Setup(r => r.GetCountsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((0, 0));

        return (mockPromptRepo, mockReactionRepo);
    }

    #region Setter

    [Fact]
    public async Task SetReactionAsync_ShouldThrow_WhenUserIdEmpty()
    {
        var (promptRepo, reactionRepo) = MakeRepos();
        var service = new PromptReactionSetterService(promptRepo.Object, reactionRepo.Object, _mapper);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SetReactionAsync(Guid.Empty, Guid.NewGuid(), ReactionType.Like));
    }

    [Fact]
    public async Task SetReactionAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var (promptRepo, reactionRepo) = MakeRepos(promptExists: false);
        var service = new PromptReactionSetterService(promptRepo.Object, reactionRepo.Object, _mapper);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            service.SetReactionAsync(Guid.NewGuid(), Guid.NewGuid(), ReactionType.Like));
    }

    [Fact]
    public async Task SetReactionAsync_ShouldAddNew_WhenNoExistingReaction()
    {
        var userId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var (promptRepo, reactionRepo) = MakeRepos();

        // after add, re-read returns the new reaction
        reactionRepo.SetupSequence(r => r.GetReactionAsync(userId, promptId))
            .ReturnsAsync((PromptReaction?)null)
            .ReturnsAsync(new PromptReaction { UserID = userId, PromptID = promptId, Type = ReactionType.Like });

        var service = new PromptReactionSetterService(promptRepo.Object, reactionRepo.Object, _mapper);

        var result = await service.SetReactionAsync(userId, promptId, ReactionType.Like);

        Assert.Equal(ReactionType.Like, result.MyReaction);
        reactionRepo.Verify(r => r.AddReactionAsync(
            It.Is<PromptReaction>(x => x.UserID == userId && x.PromptID == promptId && x.Type == ReactionType.Like)), Times.Once);
        reactionRepo.Verify(r => r.RemoveReactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SetReactionAsync_ShouldUntoggle_WhenSameReactionClickedAgain()
    {
        var userId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var existing = new PromptReaction { UserID = userId, PromptID = promptId, Type = ReactionType.Dislike };
        var (promptRepo, reactionRepo) = MakeRepos(existing: existing);

        reactionRepo.SetupSequence(r => r.GetReactionAsync(userId, promptId))
            .ReturnsAsync(existing)
            .ReturnsAsync((PromptReaction?)null); // removed on second read

        var service = new PromptReactionSetterService(promptRepo.Object, reactionRepo.Object, _mapper);

        var result = await service.SetReactionAsync(userId, promptId, ReactionType.Dislike);

        Assert.Null(result.MyReaction);
        reactionRepo.Verify(r => r.RemoveReactionAsync(userId, promptId), Times.Once);
        reactionRepo.Verify(r => r.UpdateReactionAsync(It.IsAny<PromptReaction>()), Times.Never);
    }

    [Fact]
    public async Task SetReactionAsync_ShouldSwitchLikeToDislike_WhenDifferentType()
    {
        var userId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var existing = new PromptReaction { UserID = userId, PromptID = promptId, Type = ReactionType.Like };
        var updated = new PromptReaction { UserID = userId, PromptID = promptId, Type = ReactionType.Dislike };
        var (promptRepo, reactionRepo) = MakeRepos(existing: existing);

        reactionRepo.SetupSequence(r => r.GetReactionAsync(userId, promptId))
            .ReturnsAsync(existing)
            .ReturnsAsync(updated);

        var service = new PromptReactionSetterService(promptRepo.Object, reactionRepo.Object, _mapper);

        var result = await service.SetReactionAsync(userId, promptId, ReactionType.Dislike);

        Assert.Equal(ReactionType.Dislike, result.MyReaction);
        reactionRepo.Verify(r => r.UpdateReactionAsync(
            It.Is<PromptReaction>(x => x.Type == ReactionType.Dislike)), Times.Once);
        reactionRepo.Verify(r => r.RemoveReactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SetReactionAsync_ShouldReturnCounts_FromRepository()
    {
        var (promptRepo, reactionRepo) = MakeRepos();
        reactionRepo.Setup(r => r.GetCountsAsync(It.IsAny<Guid>())).ReturnsAsync((5, 2));
        reactionRepo.Setup(r => r.GetReactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((PromptReaction?)null);

        var service = new PromptReactionSetterService(promptRepo.Object, reactionRepo.Object, _mapper);

        var result = await service.SetReactionAsync(Guid.NewGuid(), Guid.NewGuid(), ReactionType.Like);

        Assert.Equal(5, result.LikesCount);
        Assert.Equal(2, result.DislikesCount);
    }

    #endregion

    #region Remover

    [Fact]
    public async Task RemoveReactionAsync_ShouldRemove_AndReturnNullMyReaction()
    {
        var (promptRepo, reactionRepo) = MakeRepos();

        var service = new PromptReactionRemoverService(promptRepo.Object, reactionRepo.Object);

        var result = await service.RemoveReactionAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result.MyReaction);
        reactionRepo.Verify(r => r.RemoveReactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task RemoveReactionAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var (promptRepo, reactionRepo) = MakeRepos(promptExists: false);
        var service = new PromptReactionRemoverService(promptRepo.Object, reactionRepo.Object);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            service.RemoveReactionAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    #endregion

    #region Getter

    [Fact]
    public async Task GetStateAsync_ShouldReturnStateWithoutMyReaction_ForAnonymousCaller()
    {
        var (promptRepo, reactionRepo) = MakeRepos();
        reactionRepo.Setup(r => r.GetCountsAsync(It.IsAny<Guid>())).ReturnsAsync((3, 1));

        var service = new PromptReactionGetterService(promptRepo.Object, reactionRepo.Object);

        var result = await service.GetStateAsync(Guid.Empty, Guid.NewGuid());

        Assert.Equal(3, result.LikesCount);
        Assert.Equal(1, result.DislikesCount);
        Assert.Null(result.MyReaction);
        reactionRepo.Verify(r => r.GetReactionAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetStateAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var (promptRepo, reactionRepo) = MakeRepos(promptExists: false);
        var service = new PromptReactionGetterService(promptRepo.Object, reactionRepo.Object);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            service.GetStateAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    #endregion
}

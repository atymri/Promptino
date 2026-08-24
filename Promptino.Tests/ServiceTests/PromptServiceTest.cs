using AutoMapper;
using Moq;
using System.Linq.Expressions;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.Services.PromptServices;


namespace Promptino.Tests.ServiceTests;

public class PromptServiceTest
{
    private readonly IMapper _mapper;

    public PromptServiceTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();
    }

    #region Helpers
    private static Prompt MakePrompt(Guid? id = null, string title = "title", Guid? ownerId = null) =>
        new Prompt { ID = id ?? Guid.NewGuid(), Title = title, Description = "desc", Content = "content", UserID = ownerId ?? Guid.NewGuid() };

    private static Image MakeImage(Guid? id = null, string title = "img") =>
        new Image { ID = id ?? Guid.NewGuid(), Title = title, Path = "/p", GeneratedWith = "g" };
    #endregion

    // ------------------------------------------------------------
    // PromptAdderService Tests
    // ------------------------------------------------------------

    #region AdderTests

    [Fact]
    public async Task CreatePromptAsync_ShouldThrow_WhenOwnerIdEmpty()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        var service = new PromptAdderService(
            mockPromptImageRepo.Object,
            mockPromptRepo.Object,
            _mapper
        );

        var req = new PromptAddRequest("title", "desc", "content");

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePromptAsync(req, Guid.Empty));
    }

    [Fact]
    public async Task CreatePromptAsync_ShouldSetOwner_WhenValid()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        var ownerId = Guid.NewGuid();
        Prompt captured = null;

        mockPromptRepo
            .Setup(r => r.AddPromptAsync(It.IsAny<Prompt>()))
            .Callback<Prompt>(p => { p.ID = Guid.NewGuid(); captured = p; })
            .ReturnsAsync((Prompt p) => p);

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new PromptAddRequest("title", "desc", "content");

        var result = await service.CreatePromptAsync(req, ownerId);

        Assert.NotNull(result);
        Assert.Equal(ownerId, captured.UserID);
        Assert.Equal(ownerId, result.AuthorId);
        mockPromptRepo.Verify(r => r.AddPromptAsync(It.Is<Prompt>(p => p.UserID == ownerId)), Times.Once);
    }

    [Fact]
    public async Task CreatePromptAsync_ShouldThrow_WhenRequestNull()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => service.CreatePromptAsync(null, Guid.NewGuid()));
    }

    #endregion

    // ------------------------------------------------------------
    // PromptGetterService Tests
    // ------------------------------------------------------------

    #region GetterTests

    [Fact]
    public async Task GetAllPromptsAsync_ShouldReturnMappedList_WhenSomeExist()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var list = new List<Prompt> { MakePrompt(title: "A"), MakePrompt(title: "B") };

        mockPromptRepo
            .Setup(r => r.GetPromptsPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((list.Count, (IReadOnlyList<Prompt>)list));

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.GetAllPromptsAsync();

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, r => r.Title == "A");
    }

    [Fact]
    public async Task GetAllPromptsAsync_ShouldReturnEmptyList_WhenNone()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo
            .Setup(r => r.GetPromptsPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((0, (IReadOnlyList<Prompt>)new List<Prompt>()));

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.GetAllPromptsAsync();

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetPromptsByOwnerAsync_ShouldThrow_WhenUserIdEmpty()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetPromptsByOwnerAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetPromptsByOwnerAsync_ShouldReturnOwnedPromptsOnly()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var ownerId = Guid.NewGuid();

        var list = new List<Prompt>
        {
            MakePrompt(title: "mine-1", ownerId: ownerId),
            MakePrompt(title: "mine-2", ownerId: ownerId)
        };

        mockPromptRepo.Setup(r => r.GetPromptsByOwnerAsync(ownerId)).ReturnsAsync(list);

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.GetPromptsByOwnerAsync(ownerId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllPromptsAsync_ShouldIncludeCounts_AndAuthorName()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var owner = new ApplicationUser { Id = Guid.NewGuid(), UserName = "owner@example.com" };
        var prompt = MakePrompt(ownerId: owner.Id);
        prompt.User = owner;
        prompt.Reactions.Add(new PromptReaction { Type = ReactionType.Like });
        prompt.Reactions.Add(new PromptReaction { Type = ReactionType.Dislike });
        prompt.Comments.Add(new Comment { Content = "hi" });
        prompt.SavedPrompts.Add(new SavedPrompt());

        mockPromptRepo
            .Setup(r => r.GetPromptsPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((1, (IReadOnlyList<Prompt>)new List<Prompt> { prompt }));

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = (await service.GetAllPromptsAsync()).Items.Single();

        Assert.Equal(1, result.LikesCount);
        Assert.Equal(1, result.DislikesCount);
        Assert.Equal(1, result.CommentsCount);
        Assert.Equal(1, result.SavesCount);
        Assert.Equal("owner@example.com", result.AuthorName);
    }

    [Fact]
    public async Task GetPromptByConditionAsync_ShouldThrow_WhenConditionNull()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetPromptByConditionAsync(null));
    }

    [Fact]
    public async Task GetPromptByConditionAsync_ShouldReturnMappedResult_WhenFound()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var prompt = MakePrompt(title: "A");

        mockPromptRepo.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>())).ReturnsAsync(prompt);

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        Expression<Func<PromptResponse, bool>> cond = p => p.Title == "A";

        var result = await service.GetPromptByConditionAsync(cond);

        Assert.NotNull(result);
        Assert.Equal(prompt.ID, result.Id);
    }

    [Fact]
    public async Task GetPromptByConditionAsync_ShouldReturnNull_WhenNotFound()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>())).ReturnsAsync((Prompt)null);

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        Expression<Func<PromptResponse, bool>> cond = p => p.Title == "X";

        var result = await service.GetPromptByConditionAsync(cond);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchPromptsAsync_ShouldThrow_WhenKeywordNullOrWhitespace()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchPromptsAsync(null));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchPromptsAsync("   "));
    }

    [Fact]
    public async Task SearchPromptsAsync_ShouldReturnEmpty_WhenRepoReturnsEmpty()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo
            .Setup(r => r.SearchPromptPagedAsync("x", It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((0, (IReadOnlyList<Prompt>)new List<Prompt>()));

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.SearchPromptsAsync("x");

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task SearchPromptsAsync_ShouldReturnList_WhenRepoReturns()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var found = new List<Prompt> { MakePrompt(title: "test") };
        mockPromptRepo
            .Setup(r => r.SearchPromptPagedAsync("test", It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((found.Count, (IReadOnlyList<Prompt>)found));

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.SearchPromptsAsync("test");

        Assert.Single(result.Items);
    }

    #endregion

    // ------------------------------------------------------------
    // PromptUpdaterService Tests
    // ------------------------------------------------------------

    #region UpdaterTests

    [Fact]
    public async Task UpdatePromptAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
                      .ReturnsAsync((Prompt)null);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(Guid.NewGuid(), "t", "d", "c");

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            service.UpdatePromptAsync(req, Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task UpdatePromptAsync_ShouldThrowOwnershipException_WhenNotOwnerAndNotAdmin()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var existing = MakePrompt(ownerId: Guid.NewGuid());
        mockPromptRepo.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
                      .ReturnsAsync(existing);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(existing.ID, "new", "d", "c");

        await Assert.ThrowsAsync<PromptOwnershipException>(() =>
            service.UpdatePromptAsync(req, Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task UpdatePromptAsync_ShouldSucceed_WhenOwner()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var ownerId = Guid.NewGuid();
        var existing = MakePrompt(ownerId: ownerId);

        mockPromptRepo.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
                      .ReturnsAsync(existing);
        mockPromptRepo.Setup(r => r.UpdatePromptAsync(It.IsAny<Prompt>())).ReturnsAsync(existing);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(existing.ID, "new", "d", "c");

        var result = await service.UpdatePromptAsync(req, ownerId, isAdmin: false);

        Assert.NotNull(result);
        Assert.Equal(existing.ID, result.Id);
    }

    [Fact]
    public async Task UpdatePromptAsync_ShouldSucceed_WhenAdminEvenIfNotOwner()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var existing = MakePrompt(ownerId: Guid.NewGuid());

        mockPromptRepo.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
                      .ReturnsAsync(existing);
        mockPromptRepo.Setup(r => r.UpdatePromptAsync(It.IsAny<Prompt>())).ReturnsAsync(existing);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(existing.ID, "new", "d", "c");

        var result = await service.UpdatePromptAsync(req, Guid.NewGuid(), isAdmin: true);

        Assert.NotNull(result);
    }

    #endregion

    // ------------------------------------------------------------
    // PromptDeleterService Tests
    // ------------------------------------------------------------

    #region DeleterTests

    [Fact]
    public async Task DeletePromptAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(false);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            service.DeletePromptAsync(Guid.NewGuid(), Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DeletePromptAsync_ShouldThrowOwnershipException_WhenNotOwnerAndNotAdmin()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);
        mockPromptRepo.Setup(r => r.GetPromptOwnerIdAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(Guid.NewGuid());

        var service = new PromptDeleterService(mockPromptRepo.Object);

        await Assert.ThrowsAsync<PromptOwnershipException>(() =>
            service.DeletePromptAsync(Guid.NewGuid(), Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DeletePromptAsync_ShouldReturnTrue_WhenOwnerDeletes()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var ownerId = Guid.NewGuid();

        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);
        mockPromptRepo.Setup(r => r.GetPromptOwnerIdAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(ownerId);
        mockPromptRepo.Setup(r => r.DeletePromptAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        var result = await service.DeletePromptAsync(Guid.NewGuid(), ownerId, isAdmin: false);

        Assert.True(result);
        mockPromptRepo.Verify(r => r.DeletePromptAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task DeletePromptAsync_ShouldReturnTrue_WhenAdminDeletesOthersPrompt()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);
        mockPromptRepo.Setup(r => r.GetPromptOwnerIdAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(Guid.NewGuid());
        mockPromptRepo.Setup(r => r.DeletePromptAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        var result = await service.DeletePromptAsync(Guid.NewGuid(), Guid.NewGuid(), isAdmin: true);

        Assert.True(result);
    }

    [Fact]
    public async Task DeletePromptAsync_ShouldThrow_WhenRepositoryThrows()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var ownerId = Guid.NewGuid();

        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);
        mockPromptRepo.Setup(r => r.GetPromptOwnerIdAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(ownerId);
        mockPromptRepo.Setup(r => r.DeletePromptAsync(It.IsAny<Guid>()))
                      .ThrowsAsync(new InvalidOperationException("db"));

        var service = new PromptDeleterService(mockPromptRepo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeletePromptAsync(Guid.NewGuid(), ownerId, isAdmin: false));
    }

    #endregion
}

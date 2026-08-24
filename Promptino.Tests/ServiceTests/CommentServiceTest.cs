using AutoMapper;
using Moq;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.Services.CommentServices;

namespace Promptino.Tests.ServiceTests;

public class CommentServiceTest
{
    private readonly IMapper _mapper;

    public CommentServiceTest()
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    #region Adder

    [Fact]
    public async Task AddCommentAsync_ShouldThrow_WhenRequestNull()
    {
        var service = new CommentAdderService(
            new Mock<IPromptRepository>().Object, new Mock<ICommentRepository>().Object, _mapper);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddCommentAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrow_WhenUserIdEmpty()
    {
        var service = new CommentAdderService(
            new Mock<IPromptRepository>().Object, new Mock<ICommentRepository>().Object, _mapper);

        var request = new CommentAddRequest(Guid.NewGuid(), "nice prompt");

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddCommentAsync(Guid.Empty, request));
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var service = new CommentAdderService(mockPromptRepo.Object, new Mock<ICommentRepository>().Object, _mapper);

        var request = new CommentAddRequest(Guid.NewGuid(), "nice prompt");

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() => service.AddCommentAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task AddCommentAsync_ShouldReturnResponse_WithAuthorName_WhenValid()
    {
        var userId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "ali@example.com", FirstName = "Ali", LastName = "Karimi" };
        var added = new Comment { ID = Guid.NewGuid(), UserID = userId, PromptID = promptId, Content = "nice", User = user };

        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.AddAsync(It.IsAny<Comment>())).ReturnsAsync(added);

        var service = new CommentAdderService(mockPromptRepo.Object, mockCommentRepo.Object, _mapper);

        var result = await service.AddCommentAsync(userId, new CommentAddRequest(promptId, "nice"));

        Assert.NotNull(result);
        Assert.Equal("Ali Karimi", result.AuthorName);
        Assert.Equal(promptId, result.PromptId);
        mockCommentRepo.Verify(r => r.AddAsync(It.Is<Comment>(c => c.UserID == userId && c.PromptID == promptId)), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ReplySetsParentCommentID()
    {
        var userId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var rootId = Guid.NewGuid();

        var root = new Comment { ID = rootId, PromptID = promptId };

        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(rootId)).ReturnsAsync(root);

        Comment? captured = null;
        mockCommentRepo.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => captured = c)
            .ReturnsAsync(new Comment { ID = Guid.NewGuid(), UserID = userId, PromptID = promptId, Content = "rep", ParentCommentID = rootId, User = new ApplicationUser() });

        var service = new CommentAdderService(mockPromptRepo.Object, mockCommentRepo.Object, _mapper);

        var result = await service.AddCommentAsync(userId, new CommentAddRequest(promptId, "rep", rootId));

        Assert.NotNull(result);
        Assert.NotNull(captured);
        Assert.Equal(rootId, captured.ParentCommentID);
        Assert.Equal(rootId, result.ParentCommentID);
    }

    [Fact]
    public async Task AddCommentAsync_ReplyToReply_IsNormalizedToRoot()
    {
        var promptId = Guid.NewGuid();
        var rootId = Guid.NewGuid();
        var replyId = Guid.NewGuid();

        var replyToReplyTo = new Comment { ID = replyId, PromptID = promptId, ParentCommentID = rootId };

        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(replyId)).ReturnsAsync(replyToReplyTo);

        Comment? captured = null;
        mockCommentRepo.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => captured = c)
            .ReturnsAsync(new Comment { ID = Guid.NewGuid(), User = new ApplicationUser() });

        var service = new CommentAdderService(mockPromptRepo.Object, mockCommentRepo.Object, _mapper);

        await service.AddCommentAsync(Guid.NewGuid(), new CommentAddRequest(promptId, "rep", replyId));

        Assert.NotNull(captured);
        // It should have normalized to the root ID, not the reply ID
        Assert.Equal(rootId, captured.ParentCommentID);
    }

    [Fact]
    public async Task AddCommentAsync_Throws_WhenParentMissing()
    {
        var promptId = Guid.NewGuid();

        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Comment?)null);

        var service = new CommentAdderService(mockPromptRepo.Object, mockCommentRepo.Object, _mapper);

        await Assert.ThrowsAsync<CommentNotFoundException>(() =>
            service.AddCommentAsync(Guid.NewGuid(), new CommentAddRequest(promptId, "rep", Guid.NewGuid())));
    }

    [Fact]
    public async Task AddCommentAsync_Throws_WhenParentBelongsToDifferentPrompt()
    {
        var promptId = Guid.NewGuid();
        var parentFromOtherPrompt = new Comment { ID = Guid.NewGuid(), PromptID = Guid.NewGuid() };

        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(parentFromOtherPrompt);

        var service = new CommentAdderService(mockPromptRepo.Object, mockCommentRepo.Object, _mapper);

        await Assert.ThrowsAsync<CommentNotFoundException>(() =>
            service.AddCommentAsync(Guid.NewGuid(), new CommentAddRequest(promptId, "rep", Guid.NewGuid())));
    }

    #endregion

    #region Deleter

    [Fact]
    public async Task DeleteCommentAsync_ShouldThrow_WhenCommentDoesNotExist()
    {
        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Comment?)null);

        var service = new CommentDeleterService(mockCommentRepo.Object);

        await Assert.ThrowsAsync<CommentNotFoundException>(() =>
            service.DeleteCommentAsync(Guid.NewGuid(), Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldThrowOwnershipException_WhenNotAuthorAndNotAdmin()
    {
        var comment = new Comment { ID = Guid.NewGuid(), UserID = Guid.NewGuid(), Content = "c" };

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(comment.ID)).ReturnsAsync(comment);

        var service = new CommentDeleterService(mockCommentRepo.Object);

        await Assert.ThrowsAsync<CommentOwnershipException>(() =>
            service.DeleteCommentAsync(comment.ID, Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldSucceed_WhenAuthorDeletes()
    {
        var authorId = Guid.NewGuid();
        var comment = new Comment { ID = Guid.NewGuid(), UserID = authorId, Content = "c" };

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(comment.ID)).ReturnsAsync(comment);
        mockCommentRepo.Setup(r => r.DeleteAsync(comment.ID)).ReturnsAsync(true);

        var service = new CommentDeleterService(mockCommentRepo.Object);

        var result = await service.DeleteCommentAsync(comment.ID, authorId, isAdmin: false);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldSucceed_WhenAdminDeletesOthersComment()
    {
        var comment = new Comment { ID = Guid.NewGuid(), UserID = Guid.NewGuid(), Content = "c" };

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(comment.ID)).ReturnsAsync(comment);
        mockCommentRepo.Setup(r => r.DeleteAsync(comment.ID)).ReturnsAsync(true);

        var service = new CommentDeleterService(mockCommentRepo.Object);

        var result = await service.DeleteCommentAsync(comment.ID, Guid.NewGuid(), isAdmin: true);

        Assert.True(result);
    }

    #endregion

    #region Getter

    [Fact]
    public async Task GetCommentsForPromptAsync_GroupsRepliesUnderRoots_AndSetsLikesCount()
    {
        var root1 = Guid.NewGuid();
        var root2 = Guid.NewGuid();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u" };

        var comments = new List<Comment>
        {
            new Comment { ID = root1, Content = "r1", User = user },
            new Comment { ID = root2, Content = "r2", User = user },
            new Comment { ID = Guid.NewGuid(), ParentCommentID = root1, Content = "reply-to-1", User = user }
        };
        comments[0].Likes.Add(new CommentLike { UserID = Guid.NewGuid() });
        comments[0].Likes.Add(new CommentLike { UserID = Guid.NewGuid() });

        var roots = comments.Where(c => c.ParentCommentID == null).ToList();
        var replies = comments.Where(c => c.ParentCommentID != null).ToList();

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo
            .Setup(r => r.GetRootsByPromptPagedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((comments.Count, (IReadOnlyList<Comment>)roots));
        mockCommentRepo
            .Setup(r => r.GetRepliesForRootsAsync(It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(replies);

        var service = new CommentGetterService(mockCommentRepo.Object, _mapper);

        var result = (await service.GetCommentsForPromptAsync(Guid.NewGuid())).Items;

        Assert.Equal(2, result.Count); // Only roots at top level
        Assert.Contains(result, c => c.Content == "r1");
        Assert.Contains(result, c => c.Content == "r2");

        var r1 = result.Single(c => c.Content == "r1");
        Assert.Equal(2, r1.LikesCount);
        Assert.NotNull(r1.Replies);
        Assert.Single(r1.Replies);
        Assert.Equal("reply-to-1", r1.Replies.First().Content);

        var r2 = result.Single(c => c.Content == "r2");
        Assert.Equal(0, r2.LikesCount);
        Assert.Empty(r2.Replies);
    }

    [Fact]
    public async Task GetCommentsForPromptAsync_SetsIsLikedByMe_Correctly()
    {
        var currentUserId = Guid.NewGuid();
        var rootId = Guid.NewGuid();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u" };

        var comments = new List<Comment>
        {
            new Comment { ID = rootId, Content = "r", User = user }
        };
        comments[0].Likes.Add(new CommentLike { UserID = currentUserId });
        comments[0].Likes.Add(new CommentLike { UserID = Guid.NewGuid() }); // someone else

        var roots = comments.Where(c => c.ParentCommentID == null).ToList();

        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo
            .Setup(r => r.GetRootsByPromptPagedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((comments.Count, (IReadOnlyList<Comment>)roots));
        mockCommentRepo
            .Setup(r => r.GetRepliesForRootsAsync(It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(new List<Comment>());

        var service = new CommentGetterService(mockCommentRepo.Object, _mapper);

        // Call AS the user who liked it
        var resultAsLiker = (await service.GetCommentsForPromptAsync(Guid.NewGuid(), currentUserId)).Items.Single();
        Assert.True(resultAsLiker.IsLikedByMe);
        Assert.Equal(2, resultAsLiker.LikesCount);

        // Call as someone else
        var resultAsOther = (await service.GetCommentsForPromptAsync(Guid.NewGuid(), Guid.NewGuid())).Items.Single();
        Assert.False(resultAsOther.IsLikedByMe);

        // Call as anonymous
        var resultAsAnon = (await service.GetCommentsForPromptAsync(Guid.NewGuid(), null)).Items.Single();
        Assert.False(resultAsAnon.IsLikedByMe);
    }

    [Fact]
    public async Task GetCommentsForPromptAsync_ShouldReturnEmpty_WhenNone()
    {
        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo
            .Setup(r => r.GetRootsByPromptPagedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((0, (IReadOnlyList<Comment>)new List<Comment>()));
        mockCommentRepo
            .Setup(r => r.GetRepliesForRootsAsync(It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(new List<Comment>());

        var service = new CommentGetterService(mockCommentRepo.Object, _mapper);

        var result = await service.GetCommentsForPromptAsync(Guid.NewGuid());

        Assert.Empty(result.Items);
    }

    #endregion
}

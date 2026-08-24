using AutoMapper;
using Moq;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Exceptions;
using Promptino.Core.Services.CommentServices;

namespace Promptino.Tests.ServiceTests;

public class CommentLikeServiceTest
{
    private static (Mock<ICommentRepository>, Mock<ICommentLikeRepository>) MakeRepos(
        bool commentExists = true, CommentLike? existing = null)
    {
        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(commentExists ? new Comment { PromptID = Guid.Empty } : null);

        var mockLikeRepo = new Mock<ICommentLikeRepository>();
        mockLikeRepo.Setup(r => r.GetLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(existing);
        mockLikeRepo.Setup(r => r.GetCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        return (mockCommentRepo, mockLikeRepo);
    }

    #region Setter

    [Fact]
    public async Task ToggleLikeAsync_ShouldThrow_WhenCommentDoesNotExist()
    {
        var (commentRepo, likeRepo) = MakeRepos(commentExists: false);
        var service = new CommentLikeSetterService(commentRepo.Object, likeRepo.Object);

        await Assert.ThrowsAsync<CommentNotFoundException>(() =>
            service.ToggleLikeAsync(Guid.NewGuid(), Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldThrow_WhenCommentBelongsToDifferentPrompt()
    {
        var mockCommentRepo = new Mock<ICommentRepository>();
        mockCommentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Comment { PromptID = Guid.NewGuid() }); // different from requested

        var mockLikeRepo = new Mock<ICommentLikeRepository>();
        var service = new CommentLikeSetterService(mockCommentRepo.Object, mockLikeRepo.Object);

        await Assert.ThrowsAsync<CommentNotFoundException>(() =>
            service.ToggleLikeAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldAddLike_WhenNoneExists()
    {
        var userId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var promptId = Guid.Empty; // matches mock
        var (commentRepo, likeRepo) = MakeRepos();

        likeRepo.SetupSequence(r => r.GetLikeAsync(userId, commentId))
            .ReturnsAsync((CommentLike?)null)
            .ReturnsAsync(new CommentLike { UserID = userId, CommentID = commentId });

        var service = new CommentLikeSetterService(commentRepo.Object, likeRepo.Object);

        var result = await service.ToggleLikeAsync(userId, promptId, commentId);

        Assert.True(result.IsLikedByMe);
        likeRepo.Verify(r => r.AddLikeAsync(It.Is<CommentLike>(l => l.UserID == userId && l.CommentID == commentId)), Times.Once);
        likeRepo.Verify(r => r.RemoveLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldRemoveLike_WhenAlreadyLiked()
    {
        var userId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var promptId = Guid.Empty; // matches mock
        var existing = new CommentLike { UserID = userId, CommentID = commentId };
        var (commentRepo, likeRepo) = MakeRepos(existing: existing);

        likeRepo.SetupSequence(r => r.GetLikeAsync(userId, commentId))
            .ReturnsAsync(existing)
            .ReturnsAsync((CommentLike?)null);

        var service = new CommentLikeSetterService(commentRepo.Object, likeRepo.Object);

        var result = await service.ToggleLikeAsync(userId, promptId, commentId);

        Assert.False(result.IsLikedByMe);
        likeRepo.Verify(r => r.RemoveLikeAsync(userId, commentId), Times.Once);
        likeRepo.Verify(r => r.AddLikeAsync(It.IsAny<CommentLike>()), Times.Never);
    }

    #endregion

    #region Remover

    [Fact]
    public async Task RemoveLikeAsync_ShouldRemove_AndReturnState()
    {
        var (commentRepo, likeRepo) = MakeRepos();

        var service = new CommentLikeRemoverService(commentRepo.Object, likeRepo.Object);

        var result = await service.RemoveLikeAsync(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        Assert.False(result.IsLikedByMe);
        likeRepo.Verify(r => r.RemoveLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }

    #endregion
}

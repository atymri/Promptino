using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
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
        });

        _mapper = config.CreateMapper();
    }

    #region Helpers
    private static Prompt MakePrompt(Guid? id = null, string title = "title") =>
        new Prompt { ID = id ?? Guid.NewGuid(), Title = title, Description = "desc", Content = "content" };

    private static Image MakeImage(Guid? id = null, string title = "img") =>
        new Image { ID = id ?? Guid.NewGuid(), Title = title, Path = "/p", GeneratedWith = "g" };

    private static PromptResponse MakePromptResponse(Prompt p) =>
        new PromptResponse(p.ID, p.Title, p.Description, p.Content, null);
    #endregion

    // ------------------------------------------------------------
    // PromptAdderService Tests
    // ------------------------------------------------------------

    [Fact]
    public async Task CreatePromptAsync_ShouldThrow_WhenImagesMoreThanSix()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        var service = new PromptAdderService(
            mockPromptImageRepo.Object,
            mockPromptRepo.Object,
            _mapper
        );

        var images = Enumerable.Range(1, 7).Select(x => new ImageAddRequest("t", "p", "g"));
        var req = new PromptAddRequest("title", "desc", "content", images);

        await Assert.ThrowsAsync<ImageLimitException>(() => service.CreatePromptAsync(req));
    }

    [Fact]
    public async Task CreatePromptAsync_ShouldCallRepository_WhenValid_NoImages()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        var prompt = MakePrompt();
        var resp = MakePromptResponse(prompt);

        mockPromptImageRepo
            .Setup(r => r.CreatePromptWithImagesAsync(It.IsAny<PromptAddRequest>()))
            .ReturnsAsync(resp);

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new PromptAddRequest("title", "desc", "content", null);

        var result = await service.CreatePromptAsync(req);

        Assert.NotNull(result);
        mockPromptImageRepo.Verify(r => r.CreatePromptWithImagesAsync(It.Is<PromptAddRequest>(p => p.Title == "title")), Times.Once);
    }

    [Fact]
    public async Task CreatePromptAsync_ShouldCallRepository_WhenValid_WithImages()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        var prompt = MakePrompt();
        var resp = MakePromptResponse(prompt);

        mockPromptImageRepo
            .Setup(r => r.CreatePromptWithImagesAsync(It.IsAny<PromptAddRequest>()))
            .ReturnsAsync(resp);

        var imgs = new[] { new ImageAddRequest("t1", "/p1", "g1"), new ImageAddRequest("t2", "/p2", "g2") };

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new PromptAddRequest("title", "desc", "content", imgs);

        var result = await service.CreatePromptAsync(req);

        Assert.NotNull(result);
        mockPromptImageRepo.Verify(r => r.CreatePromptWithImagesAsync(It.Is<PromptAddRequest>(p => p.Images != null && p.Images.Count() == 2)), Times.Once);
    }

    [Fact]
    public async Task CreatePromptAsync_ShouldThrow_WhenPromptImageRepoThrows()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        mockPromptImageRepo
            .Setup(r => r.CreatePromptWithImagesAsync(It.IsAny<PromptAddRequest>()))
            .ThrowsAsync(new InvalidOperationException("db error"));

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new PromptAddRequest("title", "desc", "content", null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePromptAsync(req));
    }

    [Fact]
    public async Task CreatePromptAsync_ShouldReturnNullMapping_WhenRepoReturnsNull()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        mockPromptImageRepo
            .Setup(r => r.CreatePromptWithImagesAsync(It.IsAny<PromptAddRequest>()))
            .ReturnsAsync((PromptResponse)null);

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new PromptAddRequest("title", "desc", "content", null);

        var result = await service.CreatePromptAsync(req);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ShouldThrow_WhenAddToFavoritesFails()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        mockPromptRepo.Setup(r => r.AddToFavoritesAsync(It.IsAny<FavoritePrompts>())).ReturnsAsync(false);

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new FavoritePromptAddRequest(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<Exception>(() => service.AddToFavoritesAsync(req));
    }

    [Fact]
    public async Task AddToFavoritesAsync_ShouldReturnResponse_WhenSuccess()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        mockPromptRepo.Setup(r => r.AddToFavoritesAsync(It.IsAny<FavoritePrompts>())).ReturnsAsync(true);

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new FavoritePromptAddRequest(Guid.NewGuid(), Guid.NewGuid());

        var res = await service.AddToFavoritesAsync(req);

        Assert.NotNull(res);
        Assert.Equal(req.PromptID, res.PromptId);
        Assert.Equal(req.UserID, res.UserId);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ShouldThrow_WhenRepositoryThrows()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        mockPromptRepo.Setup(r => r.AddToFavoritesAsync(It.IsAny<FavoritePrompts>()))
                      .ThrowsAsync(new InvalidOperationException("db"));

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new FavoritePromptAddRequest(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddToFavoritesAsync(req));
    }

    // ------------------------------------------------------------
    // PromptGetterService Tests
    // ------------------------------------------------------------

    [Fact]
    public async Task GetAllPromptsAsync_ShouldReturnMappedList_WhenSomeExist()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var list = new List<Prompt> { MakePrompt(title: "A"), MakePrompt(title: "B") };

        mockPromptRepo.Setup(r => r.GetPromptsAsync()).ReturnsAsync(list);

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.GetAllPromptsAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Title == "A");
    }

    [Fact]
    public async Task GetAllPromptsAsync_ShouldReturnEmptyList_WhenNone()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.GetPromptsAsync()).ReturnsAsync(new List<Prompt>());

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.GetAllPromptsAsync();

        Assert.Empty(result);
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
    public async Task GetPromptsByConditionAsync_ShouldReturnList_WhenMatches()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var data = new List<Prompt> { MakePrompt(Guid.NewGuid(), "A"), MakePrompt(Guid.NewGuid(), "B") };

        mockPromptRepo.Setup(r => r.GetPromptsByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>())).ReturnsAsync(data);

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        Expression<Func<PromptResponse, bool>> cond = p => p.Title.Contains("A") || p.Title.Contains("B");

        var result = await service.GetPromptsByConditionAsync(cond);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFavoritePromptsAsync_ShouldReturnMappedFavorites()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var favs = new List<FavoritePrompts>
        {
            new FavoritePrompts { ID = Guid.NewGuid(), UserID = Guid.NewGuid(), Prompt = MakePrompt(title:"Fav1") }
        };

        mockPromptRepo.Setup(r => r.GetFavoritePromptsAsync(It.IsAny<Guid>())).ReturnsAsync(favs);

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.GetFavoritePromptsAsync(Guid.NewGuid());

        Assert.Single(result);
    }

    [Fact]
    public async Task GetFavoritePromptsAsync_ShouldReturnEmpty_WhenNone()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.GetFavoritePromptsAsync(It.IsAny<Guid>())).ReturnsAsync(new List<FavoritePrompts>());

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.GetFavoritePromptsAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task IsPromptFavoriteAsync_ShouldThrow_WhenUserIdEmpty()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        await Assert.ThrowsAsync<ArgumentException>(() => service.IsPromptFavoriteAsync(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public async Task IsPromptFavoriteAsync_ShouldThrow_WhenPromptIdEmpty()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        await Assert.ThrowsAsync<ArgumentException>(() => service.IsPromptFavoriteAsync(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public async Task IsPromptFavoriteAsync_ShouldReturnTrue_WhenRepoSaysTrue()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.IsPromptFavoriteAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.True(result);
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
        mockPromptRepo.Setup(r => r.SearchPromptAsync("x")).ReturnsAsync(new List<Prompt>());

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.SearchPromptsAsync("x");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchPromptsAsync_ShouldReturnList_WhenRepoReturns()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.SearchPromptAsync("test")).ReturnsAsync(new List<Prompt> { MakePrompt(title: "test") });

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = await service.SearchPromptsAsync("test");

        Assert.Single(result);
    }

    // ------------------------------------------------------------
    // PromptUpdaterService Tests
    // ------------------------------------------------------------

    [Fact]
    public async Task UpdatePromptAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(Guid.NewGuid(), "t", "d", "c");

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() => service.UpdatePromptAsync(req));
    }

    [Fact]
    public async Task UpdatePromptAsync_ShouldReturnMappedResult_WhenSuccess()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var updated = MakePrompt();
        mockPromptRepo.Setup(r => r.UpdatePromptAsync(It.IsAny<Prompt>())).ReturnsAsync(updated);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(updated.ID, "new", "d", "c");

        var result = await service.UpdatePromptAsync(req);

        Assert.NotNull(result);
        Assert.Equal(updated.ID, result.Id);
    }

    [Fact]
    public async Task UpdatePromptAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        mockPromptRepo.Setup(r => r.UpdatePromptAsync(It.IsAny<Prompt>())).ReturnsAsync((Prompt)null);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(Guid.NewGuid(), "new", "d", "c");

        var result = await service.UpdatePromptAsync(req);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePromptAsync_ShouldThrow_WhenRepositoryThrows()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        mockPromptRepo.Setup(r => r.UpdatePromptAsync(It.IsAny<Prompt>())).ThrowsAsync(new InvalidOperationException("db"));

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(Guid.NewGuid(), "new", "d", "c");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePromptAsync(req));
    }

    // ------------------------------------------------------------
    // NEW TESTS – Ensure Images are mapped as ImageResponse everywhere
    // ------------------------------------------------------------

    [Fact]
    public async Task CreatePromptAsync_ShouldReturnImages_AsImageResponse()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        var mockPromptImageRepo = new Mock<IPromptImageRepository>();

        var prompt = MakePrompt();
        var images = new List<ImageResponse>
    {
        new ImageResponse(Guid.NewGuid(), "i1", "/p1", "g1"),
        new ImageResponse(Guid.NewGuid(), "i2", "/p2", "g2")
    };

        var resp = new PromptResponse(prompt.ID, prompt.Title, prompt.Description, prompt.Content, images);

        mockPromptImageRepo
            .Setup(r => r.CreatePromptWithImagesAsync(It.IsAny<PromptAddRequest>()))
            .ReturnsAsync(resp);

        var service = new PromptAdderService(mockPromptImageRepo.Object, mockPromptRepo.Object, _mapper);

        var req = new PromptAddRequest("title", "desc", "content",
            new[] { new ImageAddRequest("i1", "/p1", "g1"), new ImageAddRequest("i2", "/p2", "g2") });

        var result = await service.CreatePromptAsync(req);

        Assert.NotNull(result.Images);
        Assert.All(result.Images, img => Assert.IsType<ImageResponse>(img));
    }

    [Fact]
    public async Task GetAllPromptsAsync_ShouldReturnImages_AsImageResponse()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        var prompt = MakePrompt();
        prompt.PromptImages = new List<PromptImage>
    {
        new PromptImage { Image = MakeImage(title: "img1") },
        new PromptImage { Image = MakeImage(title: "img2") }
    };

        mockPromptRepo.Setup(r => r.GetPromptsAsync())
                      .ReturnsAsync(new List<Prompt> { prompt });

        var service = new PromptGetterService(mockPromptRepo.Object, _mapper);

        var result = (await service.GetAllPromptsAsync()).ToList();

        Assert.NotNull(result[0].Images);
        Assert.All(result[0].Images, img => Assert.IsType<ImageResponse>(img));
    }

    [Fact]
    public async Task UpdatePromptAsync_ShouldReturnImages_AsImageResponse()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var prompt = MakePrompt();
        prompt.PromptImages = new List<PromptImage>
    {
        new PromptImage { Image = MakeImage(title: "old-img") }
    };

        mockPromptRepo.Setup(r => r.UpdatePromptAsync(It.IsAny<Prompt>()))
                      .ReturnsAsync(prompt);

        var service = new PromptUpdaterService(mockPromptRepo.Object, _mapper);

        var req = new PromptUpdateRequest(prompt.ID, "new", "d", "c");

        var result = await service.UpdatePromptAsync(req);

        Assert.NotNull(result.Images);
        Assert.All(result.Images, img => Assert.IsType<ImageResponse>(img));
    }

    // ------------------------------------------------------------
    // PromptDeleterService Tests
    // ------------------------------------------------------------

    [Fact]
    public async Task DeletePromptAsync_ShouldThrow_WhenPromptDoesNotExist()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();
        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(false);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            service.DeletePromptAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeletePromptAsync_ShouldReturnTrue_WhenSuccessful()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        mockPromptRepo.Setup(r => r.DeletePromptAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        var result = await service.DeletePromptAsync(Guid.NewGuid());

        Assert.True(result);
        mockPromptRepo.Verify(r => r.DeletePromptAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task DeletePromptAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        mockPromptRepo.Setup(r => r.DeletePromptAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(false);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        var result = await service.DeletePromptAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task DeletePromptAsync_ShouldThrow_WhenRepositoryThrows()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.DoesPromptExistAsync(It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        mockPromptRepo.Setup(r => r.DeletePromptAsync(It.IsAny<Guid>()))
                      .ThrowsAsync(new InvalidOperationException("db"));

        var service = new PromptDeleterService(mockPromptRepo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeletePromptAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldThrow_WhenPromptNotFavorite()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(false);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(() =>
            service.RemoveFromFavoritesAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldReturnTrue_WhenSuccessful()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        mockPromptRepo.Setup(r => r.RemoveFromFavoritesAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        var result = await service.RemoveFromFavoritesAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.True(result);
        mockPromptRepo.Verify(r => r.RemoveFromFavoritesAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        mockPromptRepo.Setup(r => r.RemoveFromFavoritesAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(false);

        var service = new PromptDeleterService(mockPromptRepo.Object);

        var result = await service.RemoveFromFavoritesAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldThrow_WhenRepositoryThrows()
    {
        var mockPromptRepo = new Mock<IPromptRepository>();

        mockPromptRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(true);

        mockPromptRepo.Setup(r => r.RemoveFromFavoritesAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ThrowsAsync(new InvalidOperationException("db"));

        var service = new PromptDeleterService(mockPromptRepo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RemoveFromFavoritesAsync(Guid.NewGuid(), Guid.NewGuid()));
    }


}

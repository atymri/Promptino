using AutoMapper;
using Moq;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Core.Exceptions;
using Promptino.Core.Services.ImageServices;
using System.Linq.Expressions;
using Xunit;

namespace Promptino.Tests.Services;

public class ImageServicesTests
{
    private readonly Mock<IImageRepository> _imageRepositoryMock;
    private readonly Mock<IPromptRepository> _promptRepositoryMock;
    private readonly IMapper _mapper;

    public ImageServicesTests()
    {
        _imageRepositoryMock = new Mock<IImageRepository>();
        _promptRepositoryMock = new Mock<IPromptRepository>();

        // تنظیم AutoMapper واقعی برای Expression Mapping
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Image, ImageResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID));
            cfg.CreateMap<ImageResponse, Image>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id));
            cfg.CreateMap<ImageAddRequest, Image>();
            cfg.CreateMap<ImageUpdateRequest, Image>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id));
        });
        _mapper = config.CreateMapper();
    }

    #region ImageAdderService Tests

    [Fact]
    public async Task CreateImageAsync_ValidRequest_ReturnsImageResponse()
    {
        var imageRequest = new ImageAddRequest
        {
            Path = "/images/test.jpg",
            Title = "Test Image",
            GeneratedWith = "DALL-E"
        };

        var image = new Image
        {
            ID = Guid.NewGuid(),
            Path = imageRequest.Path,
            Title = imageRequest.Title,
            GeneratedWith = imageRequest.GeneratedWith
        };

        _imageRepositoryMock.Setup(r => r.AddImageAsync(It.IsAny<Image>())).ReturnsAsync(image);

        var service = new ImageAdderService(_imageRepositoryMock.Object, _promptRepositoryMock.Object, _mapper);
        var result = await service.CreateImageAsync(imageRequest);

        Assert.NotNull(result);
        Assert.Equal(image.ID, result.Id);
        Assert.Equal(image.Path, result.Path);
        Assert.Equal(image.Title, result.Title);
        Assert.Equal(image.GeneratedWith, result.GeneratedWith);
    }

    [Fact]
    public async Task CreateImageAsync_NullResult_ReturnsNull()
    {
        var imageRequest = new ImageAddRequest
        {
            Path = "/images/test.jpg",
            Title = "Test Image",
            GeneratedWith = "DALL-E"
        };

        _imageRepositoryMock.Setup(r => r.AddImageAsync(It.IsAny<Image>())).ReturnsAsync((Image?)null);

        var service = new ImageAdderService(_imageRepositoryMock.Object, _promptRepositoryMock.Object, _mapper);
        var result = await service.CreateImageAsync(imageRequest);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddImageToPromptAsync_ValidIds_ReturnsTrue()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var prompt = new Prompt { ID = promptId };

        _promptRepositoryMock.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
            .ReturnsAsync(prompt);
        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.AddImageToPromptAsync(promptId, imageId)).ReturnsAsync(true);

        var service = new ImageAdderService(_imageRepositoryMock.Object, _promptRepositoryMock.Object, _mapper);
        var result = await service.AddImageToPromptAsync(promptId, imageId);

        Assert.True(result);
    }

    [Fact]
    public async Task AddImageToPromptAsync_PromptNotFound_ThrowsPromptNotFoundException()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _promptRepositoryMock.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
            .ReturnsAsync((Prompt?)null);

        var service = new ImageAdderService(_imageRepositoryMock.Object, _promptRepositoryMock.Object, _mapper);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(async () =>
            await service.AddImageToPromptAsync(promptId, imageId));
    }

    [Fact]
    public async Task AddImageToPromptAsync_ImageNotFound_ThrowsImageNotFoundException()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var prompt = new Prompt { ID = promptId };

        _promptRepositoryMock.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
            .ReturnsAsync(prompt);
        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(false);

        var service = new ImageAdderService(_imageRepositoryMock.Object, _promptRepositoryMock.Object, _mapper);

        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
            await service.AddImageToPromptAsync(promptId, imageId));
    }

    [Fact]
    public async Task AddImageToPromptAsync_RepositoryReturnsFalse_ReturnsFalse()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var prompt = new Prompt { ID = promptId };

        _promptRepositoryMock.Setup(r => r.GetPromptByConditionAsync(It.IsAny<Expression<Func<Prompt, bool>>>()))
            .ReturnsAsync(prompt);
        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.AddImageToPromptAsync(promptId, imageId)).ReturnsAsync(false);

        var service = new ImageAdderService(_imageRepositoryMock.Object, _promptRepositoryMock.Object, _mapper);
        var result = await service.AddImageToPromptAsync(promptId, imageId);

        Assert.False(result);
    }

    #endregion

    #region ImageDeleterService Tests

    [Fact]
    public async Task DeleteImageAsync_ValidId_ReturnsTrue()
    {
        var imageId = Guid.NewGuid();

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.DeleteImageAsync(imageId)).ReturnsAsync(true);

        var service = new ImageDeleterService(_imageRepositoryMock.Object, _promptRepositoryMock.Object);
        var result = await service.DeleteImageAsync(imageId);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteImageAsync_ImageNotFound_ThrowsImageNotFoundException()
    {
        var imageId = Guid.NewGuid();

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(false);

        var service = new ImageDeleterService(_imageRepositoryMock.Object, _promptRepositoryMock.Object);

        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
            await service.DeleteImageAsync(imageId));
    }

    [Fact]
    public async Task DeleteImageAsync_RepositoryReturnsFalse_ReturnsFalse()
    {
        var imageId = Guid.NewGuid();

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.DeleteImageAsync(imageId)).ReturnsAsync(false);

        var service = new ImageDeleterService(_imageRepositoryMock.Object, _promptRepositoryMock.Object);
        var result = await service.DeleteImageAsync(imageId);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveImageFromPromptAsync_ValidIds_ReturnsTrue()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _promptRepositoryMock.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.RemoveImageFromPromptAsync(promptId, imageId)).ReturnsAsync(true);

        var service = new ImageDeleterService(_imageRepositoryMock.Object, _promptRepositoryMock.Object);
        var result = await service.RemoveImageFromPromptAsync(promptId, imageId);

        Assert.True(result);
    }

    [Fact]
    public async Task RemoveImageFromPromptAsync_PromptNotFound_ThrowsPromptNotFoundException()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _promptRepositoryMock.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(false);

        var service = new ImageDeleterService(_imageRepositoryMock.Object, _promptRepositoryMock.Object);

        await Assert.ThrowsAsync<PromptNotFoundExceptions>(async () =>
            await service.RemoveImageFromPromptAsync(promptId, imageId));
    }

    [Fact]
    public async Task RemoveImageFromPromptAsync_ImageNotFound_ThrowsImageNotFoundException()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _promptRepositoryMock.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(false);

        var service = new ImageDeleterService(_imageRepositoryMock.Object, _promptRepositoryMock.Object);

        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
            await service.RemoveImageFromPromptAsync(promptId, imageId));
    }

    [Fact]
    public async Task RemoveImageFromPromptAsync_RepositoryReturnsFalse_ReturnsFalse()
    {
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _promptRepositoryMock.Setup(r => r.DoesPromptExistAsync(promptId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.RemoveImageFromPromptAsync(promptId, imageId)).ReturnsAsync(false);

        var service = new ImageDeleterService(_imageRepositoryMock.Object, _promptRepositoryMock.Object);
        var result = await service.RemoveImageFromPromptAsync(promptId, imageId);

        Assert.False(result);
    }

    #endregion

    #region ImageGetterService Tests

    [Fact]
    public async Task GetAllImagesAsync_ImagesExist_ReturnsImageResponseList()
    {
        var images = new List<Image>
        {
            new Image { ID = Guid.NewGuid(), Path = "/img1.jpg", Title = "Image 1", GeneratedWith = "DALL-E" },
            new Image { ID = Guid.NewGuid(), Path = "/img2.jpg", Title = "Image 2", GeneratedWith = "MidJourney" }
        };

        _imageRepositoryMock.Setup(r => r.GetImagesAsync()).ReturnsAsync(images);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.GetAllImagesAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllImagesAsync_NoImages_ReturnsEmptyList()
    {
        var images = new List<Image>();

        _imageRepositoryMock.Setup(r => r.GetImagesAsync()).ReturnsAsync(images);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.GetAllImagesAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetImageByConditionAsync_ImageExists_ReturnsImageResponse()
    {
        var imageId = Guid.NewGuid();
        Expression<Func<ImageResponse, bool>> condition = x => x.Id == imageId;

        var image = new Image
        {
            ID = imageId,
            Path = "/test.jpg",
            Title = "Test",
            GeneratedWith = "DALL-E"
        };

        _imageRepositoryMock.Setup(r => r.GetImageByConditionAsync(It.IsAny<Expression<Func<Image, bool>>>()))
            .ReturnsAsync(image);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.GetImageByConditionAsync(condition);

        Assert.NotNull(result);
        Assert.Equal(imageId, result.Id);
    }

    [Fact]
    public async Task GetImageByConditionAsync_ImageNotFound_ThrowsImageNotFoundException()
    {
        var imageId = Guid.NewGuid();
        Expression<Func<ImageResponse, bool>> condition = x => x.Id == imageId;

        _imageRepositoryMock.Setup(r => r.GetImageByConditionAsync(It.IsAny<Expression<Func<Image, bool>>>()))
            .ReturnsAsync((Image?)null);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);

        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
            await service.GetImageByConditionAsync(condition));
    }

    [Fact]
    public async Task GetImagesByConditionAsync_ImagesExist_ReturnsImageResponseList()
    {
        Expression<Func<ImageResponse, bool>> condition = x => x.GeneratedWith == "DALL-E";

        var images = new List<Image>
        {
            new Image { ID = Guid.NewGuid(), Path = "/img1.jpg", Title = "Image 1", GeneratedWith = "DALL-E" }
        };

        _imageRepositoryMock.Setup(r => r.GetImagesByConditionAsync(It.IsAny<Expression<Func<Image, bool>>>()))
            .ReturnsAsync(images);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.GetImagesByConditionAsync(condition);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetImagesByConditionAsync_NoMatchingImages_ReturnsEmptyList()
    {
        Expression<Func<ImageResponse, bool>> condition = x => x.GeneratedWith == "Unknown";

        var images = new List<Image>();

        _imageRepositoryMock.Setup(r => r.GetImagesByConditionAsync(It.IsAny<Expression<Func<Image, bool>>>()))
            .ReturnsAsync(images);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.GetImagesByConditionAsync(condition);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetImagesByPromptIdAsync_ImagesExist_ReturnsImageResponseList()
    {
        var promptId = Guid.NewGuid();
        var images = new List<Image>
        {
            new Image { ID = Guid.NewGuid(), Path = "/img1.jpg", Title = "Image 1", GeneratedWith = "DALL-E" },
            new Image { ID = Guid.NewGuid(), Path = "/img2.jpg", Title = "Image 2", GeneratedWith = "MidJourney" }
        };

        _imageRepositoryMock.Setup(r => r.GetImagesByPromptIdAsync(promptId)).ReturnsAsync(images);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.GetImagesByPromptIdAsync(promptId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetImagesByPromptIdAsync_NoImages_ReturnsEmptyList()
    {
        var promptId = Guid.NewGuid();
        var images = new List<Image>();

        _imageRepositoryMock.Setup(r => r.GetImagesByPromptIdAsync(promptId)).ReturnsAsync(images);

        var service = new ImageGetterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.GetImagesByPromptIdAsync(promptId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region ImageUpdaterService Tests

    [Fact]
    public async Task UpdateImageAsync_ValidRequest_UpdatesAllProperties()
    {
        var imageId = Guid.NewGuid();
        var updateRequest = new ImageUpdateRequest
        {
            Id = imageId,
            Path = "/updated/path.jpg",
            Title = "Updated Title",
            GeneratedWith = "Updated Generator"
        };

        var updatedImage = new Image
        {
            ID = imageId,
            Path = updateRequest.Path,
            Title = updateRequest.Title,
            GeneratedWith = updateRequest.GeneratedWith
        };

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.UpdateImageAsync(It.IsAny<Image>())).ReturnsAsync(updatedImage);

        var service = new ImageUpdaterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.UpdateImageAsync(updateRequest);

        Assert.NotNull(result);
        Assert.Equal(updateRequest.Path, result.Path);
        Assert.Equal(updateRequest.Title, result.Title);
        Assert.Equal(updateRequest.GeneratedWith, result.GeneratedWith);
    }

    [Fact]
    public async Task UpdateImageAsync_UpdatePath_OnlyPathChanged()
    {
        var imageId = Guid.NewGuid();
        var updateRequest = new ImageUpdateRequest
        {
            Id = imageId,
            Path = "/new/path.jpg",
            Title = "Same Title",
            GeneratedWith = "Same Generator"
        };

        var updatedImage = new Image
        {
            ID = imageId,
            Path = "/new/path.jpg",
            Title = "Same Title",
            GeneratedWith = "Same Generator"
        };

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.UpdateImageAsync(It.IsAny<Image>())).ReturnsAsync(updatedImage);

        var service = new ImageUpdaterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.UpdateImageAsync(updateRequest);

        Assert.NotNull(result);
        Assert.Equal("/new/path.jpg", result.Path);
        Assert.Equal("Same Title", result.Title);
        Assert.Equal("Same Generator", result.GeneratedWith);
    }

    [Fact]
    public async Task UpdateImageAsync_UpdateTitle_OnlyTitleChanged()
    {
        var imageId = Guid.NewGuid();
        var updateRequest = new ImageUpdateRequest
        {
            Id = imageId,
            Path = "/same/path.jpg",
            Title = "New Title",
            GeneratedWith = "Same Generator"
        };

        var updatedImage = new Image
        {
            ID = imageId,
            Path = "/same/path.jpg",
            Title = "New Title",
            GeneratedWith = "Same Generator"
        };

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.UpdateImageAsync(It.IsAny<Image>())).ReturnsAsync(updatedImage);

        var service = new ImageUpdaterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.UpdateImageAsync(updateRequest);

        Assert.NotNull(result);
        Assert.Equal("/same/path.jpg", result.Path);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("Same Generator", result.GeneratedWith);
    }

    [Fact]
    public async Task UpdateImageAsync_UpdateGeneratedWith_OnlyGeneratedWithChanged()
    {
        var imageId = Guid.NewGuid();
        var updateRequest = new ImageUpdateRequest
        {
            Id = imageId,
            Path = "/same/path.jpg",
            Title = "Same Title",
            GeneratedWith = "New Generator"
        };

        var updatedImage = new Image
        {
            ID = imageId,
            Path = "/same/path.jpg",
            Title = "Same Title",
            GeneratedWith = "New Generator"
        };

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.UpdateImageAsync(It.IsAny<Image>())).ReturnsAsync(updatedImage);

        var service = new ImageUpdaterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.UpdateImageAsync(updateRequest);

        Assert.NotNull(result);
        Assert.Equal("/same/path.jpg", result.Path);
        Assert.Equal("Same Title", result.Title);
        Assert.Equal("New Generator", result.GeneratedWith);
    }

    [Fact]
    public async Task UpdateImageAsync_ImageNotFound_ThrowsImageNotFoundException()
    {
        var imageId = Guid.NewGuid();
        var updateRequest = new ImageUpdateRequest
        {
            Id = imageId,
            Path = "/path.jpg",
            Title = "Title",
            GeneratedWith = "Generator"
        };

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(false);

        var service = new ImageUpdaterService(_imageRepositoryMock.Object, _mapper);

        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
            await service.UpdateImageAsync(updateRequest));
    }

    [Fact]
    public async Task UpdateImageAsync_RepositoryReturnsNull_ReturnsNull()
    {
        var imageId = Guid.NewGuid();
        var updateRequest = new ImageUpdateRequest
        {
            Id = imageId,
            Path = "/path.jpg",
            Title = "Title",
            GeneratedWith = "Generator"
        };

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.UpdateImageAsync(It.IsAny<Image>())).ReturnsAsync((Image?)null);

        var service = new ImageUpdaterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.UpdateImageAsync(updateRequest);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateImageAsync_EmptyValues_UpdatesWithEmptyStrings()
    {
        var imageId = Guid.NewGuid();
        var updateRequest = new ImageUpdateRequest
        {
            Id = imageId,
            Path = "",
            Title = "",
            GeneratedWith = ""
        };

        var updatedImage = new Image
        {
            ID = imageId,
            Path = "",
            Title = "",
            GeneratedWith = ""
        };

        _imageRepositoryMock.Setup(r => r.DoesImageExistAsync(imageId)).ReturnsAsync(true);
        _imageRepositoryMock.Setup(r => r.UpdateImageAsync(It.IsAny<Image>())).ReturnsAsync(updatedImage);

        var service = new ImageUpdaterService(_imageRepositoryMock.Object, _mapper);
        var result = await service.UpdateImageAsync(updateRequest);

        Assert.NotNull(result);
        Assert.Equal("", result.Path);
        Assert.Equal("", result.Title);
        Assert.Equal("", result.GeneratedWith);
    }

    #endregion
}
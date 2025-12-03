using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Infrastructure.DatabaseContext;
using Promptino.Infrastructure.Repositories;
using System.Linq.Expressions;

namespace Promptino.Infrastructure.Tests.Repositories;

public class ImageRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ImageRepositorry _repository;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public ImageRepositoryTests()
    {
        // Create in-memory database options
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        // Create context and repository
        _context = new ApplicationDbContext(_options);
        _repository = new ImageRepositorry(_context);

        // Seed initial data
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        // Add test prompts with ALL required properties
        var prompts = new List<Prompt>
            {
                new Prompt
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Prompt 1",
                    Description = "Description 1",
                    Content = "Content 1",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Prompt
                {
                    ID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Title = "Prompt 2",
                    Description = "Description 2",
                    Content = "Content 2",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Prompt
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Title = "Prompt 3",
                    Description = "Description 3",
                    Content = "Content 3",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                }
            };

        // Add test images with all required properties
        var images = new List<Image>
            {
                new Image
                {
                    ID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Title = "Image 1",
                    Path = "/path1",
                    GeneratedWith = "AI 1",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Image
                {
                    ID = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Title = "Image 2",
                    Path = "/path2",
                    GeneratedWith = "AI 2",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Image
                {
                    ID = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Title = "Image 3",
                    Path = "/path3",
                    GeneratedWith = "AI 3",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Image
                {
                    ID = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Title = "Image 4",
                    Path = "/path4",
                    GeneratedWith = "AI 4",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                }
            };

        // Add prompt-image relationships
        var promptImages = new List<PromptImage>
            {
                new PromptImage
                {
                    ID = Guid.NewGuid(),
                    PromptID = prompts[0].ID,
                    ImageID = images[0].ID,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new PromptImage
                {
                    ID = Guid.NewGuid(),
                    PromptID = prompts[0].ID,
                    ImageID = images[1].ID,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new PromptImage
                {
                    ID = Guid.NewGuid(),
                    PromptID = prompts[1].ID,
                    ImageID = images[2].ID,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new PromptImage
                {
                    ID = Guid.NewGuid(),
                    PromptID = prompts[2].ID,
                    ImageID = images[0].ID,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                }
            };

        _context.Prompts.AddRange(prompts);
        _context.Images.AddRange(images);
        _context.PromptImages.AddRange(promptImages);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddImageAsync_ValidImage_ReturnsAddedImage()
    {
        // Arrange
        var newImage = new Image
        {
            ID = Guid.NewGuid(),
            Title = "New Image",
            Path = "/new/path",
            GeneratedWith = "DALL-E",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddImageAsync(newImage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newImage.ID, result.ID);
        Assert.Equal(newImage.Title, result.Title);
        Assert.Equal(newImage.Path, result.Path);
        Assert.Equal(newImage.GeneratedWith, result.GeneratedWith);

        // Verify it was saved to database
        var savedImage = await _context.Images.FindAsync(newImage.ID);
        Assert.NotNull(savedImage);
    }

    [Fact]
    public async Task AddImageAsync_NullImage_ThrowsException()
    {
        // Arrange
        Image nullImage = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddImageAsync(nullImage));
    }

    [Fact]
    public async Task AddImageToPromptAsync_ValidIds_ReturnsTrue()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var imageId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        // Act
        var result = await _repository.AddImageToPromptAsync(promptId, imageId);

        // Assert
        Assert.True(result);

        // Verify the relationship was created
        var promptImage = await _context.PromptImages
            .FirstOrDefaultAsync(pi => pi.PromptID == promptId && pi.ImageID == imageId);
        Assert.NotNull(promptImage);
    }

    [Fact]
    public async Task DeleteImageAsync_ExistingImage_ReturnsTrue()
    {
        // Arrange
        var imageId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Verify image exists first
        var imageExists = await _context.Images.AnyAsync(i => i.ID == imageId);
        Assert.True(imageExists);

        // Act
        var result = await _repository.DeleteImageAsync(imageId);

        // Assert
        Assert.True(result);

        // Verify image was deleted
        var deletedImage = await _context.Images.FindAsync(imageId);
        Assert.Null(deletedImage);

        // Verify related prompt images were deleted (cascade delete)
        var relatedPromptImages = await _context.PromptImages
            .Where(pi => pi.ImageID == imageId)
            .ToListAsync();
        Assert.Empty(relatedPromptImages);
    }

    [Fact]
    public async Task DeleteImageAsync_NonExistentImage_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteImageAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DoesImageExistAsync_ExistingImage_ReturnsTrue()
    {
        // Arrange
        var existingImageId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var result = await _repository.DoesImageExistAsync(existingImageId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DoesImageExistAsync_NonExistentImage_ReturnsFalse()
    {
        // Arrange
        var nonExistentImageId = Guid.NewGuid();

        // Act
        var result = await _repository.DoesImageExistAsync(nonExistentImageId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetImageByConditionAsync_ValidCondition_ReturnsImage()
    {
        // Arrange
        Expression<Func<Image, bool>> condition = i => i.Title == "Image 1";

        // Act
        var result = await _repository.GetImageByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Image 1", result.Title);
        Assert.NotNull(result.PromptImages);
        Assert.NotEmpty(result.PromptImages);
    }

    [Fact]
    public async Task GetImageByConditionAsync_WithPromptInclusion_ReturnsImageWithPrompt()
    {
        // Arrange
        var imageId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        Expression<Func<Image, bool>> condition = i => i.ID == imageId;

        // Act
        var result = await _repository.GetImageByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PromptImages);
        Assert.NotEmpty(result.PromptImages);
        Assert.All(result.PromptImages, pi => Assert.NotNull(pi.Prompt));
    }

    [Fact]
    public async Task GetImageByConditionAsync_NoMatch_ReturnsNull()
    {
        // Arrange
        Expression<Func<Image, bool>> condition = i => i.Title == "Non Existent Image";

        // Act
        var result = await _repository.GetImageByConditionAsync(condition);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetImagesAsync_ReturnsAllImagesWithPromptImages()
    {
        // Act
        var result = await _repository.GetImagesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count()); // Should have 4 seeded images

        // Verify all images have PromptImages loaded
        Assert.All(result, image => Assert.NotNull(image.PromptImages));

        // Verify Prompt is loaded in PromptImages
        Assert.All(result.SelectMany(i => i.PromptImages),
            pi => Assert.NotNull(pi.Prompt));
    }

    [Fact]
    public async Task GetImagesByConditionAsync_ValidCondition_ReturnsFilteredImages()
    {
        // Arrange
        Expression<Func<Image, bool>> condition = i => i.GeneratedWith.Contains("AI");

        // Act
        var result = await _repository.GetImagesByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, image => Assert.Contains("AI", image.GeneratedWith));
        Assert.All(result, image => Assert.NotNull(image.PromptImages));
    }

    [Fact]
    public async Task GetImagesByConditionAsync_MultipleConditions_ReturnsFilteredImages()
    {
        // Arrange
        Expression<Func<Image, bool>> condition = i =>
            i.GeneratedWith == "AI 1" || i.GeneratedWith == "AI 2";

        // Act
        var result = await _repository.GetImagesByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, image =>
            Assert.True(image.GeneratedWith == "AI 1" || image.GeneratedWith == "AI 2"));
    }

    [Fact]
    public async Task GetImagesByConditionAsync_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        Expression<Func<Image, bool>> condition = i => i.Title == "Non Existent";

        // Act
        var result = await _repository.GetImagesByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetImagesByPromptIdAsync_ValidPromptId_ReturnsImages()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _repository.GetImagesByPromptIdAsync(promptId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count()); // Prompt 1 has 2 images
        Assert.Contains(result, i => i.Title == "Image 1");
        Assert.Contains(result, i => i.Title == "Image 2");
    }

    [Fact]
    public async Task GetImagesByPromptIdAsync_PromptWithNoImages_ReturnsEmptyList()
    {
        // Arrange - Create a prompt with no images
        var newPrompt = new Prompt
        {
            ID = Guid.NewGuid(),
            Title = "Empty Prompt",
            Description = "Empty Description",
            Content = "Empty Content",
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
        _context.Prompts.Add(newPrompt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetImagesByPromptIdAsync(newPrompt.ID);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetImagesByPromptIdAsync_NonExistentPromptId_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentPromptId = Guid.NewGuid();

        // Act
        var result = await _repository.GetImagesByPromptIdAsync(nonExistentPromptId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task RemoveImageFromPromptAsync_ExistingRelationship_ReturnsTrue()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var imageId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Verify relationship exists
        var existsBefore = await _context.PromptImages
            .AnyAsync(pi => pi.PromptID == promptId && pi.ImageID == imageId);
        Assert.True(existsBefore);

        // Act
        var result = await _repository.RemoveImageFromPromptAsync(promptId, imageId);

        // Assert
        Assert.True(result);

        // Verify relationship was removed
        var existsAfter = await _context.PromptImages
            .AnyAsync(pi => pi.PromptID == promptId && pi.ImageID == imageId);
        Assert.False(existsAfter);
    }

    [Fact]
    public async Task RemoveImageFromPromptAsync_NonExistentRelationship_ReturnsFalse()
    {
        // Arrange
        var promptId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        // Act
        var result = await _repository.RemoveImageFromPromptAsync(promptId, imageId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateImageAsync_ExistingImage_ReturnsUpdatedImage()
    {
        // Arrange
        var imageId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var updatedImage = new Image
        {
            ID = imageId,
            Title = "Updated Title",
            Path = "/updated/path",
            GeneratedWith = "Updated AI"
        };

        // Act
        var result = await _repository.UpdateImageAsync(updatedImage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("/updated/path", result.Path);
        Assert.Equal("Updated AI", result.GeneratedWith);
        Assert.NotNull(result.LastUpdatedAt); // Touch() should have been called

        // Verify changes were saved
        var savedImage = await _context.Images.FindAsync(imageId);
        Assert.Equal("Updated Title", savedImage.Title);
        Assert.Equal("/updated/path", savedImage.Path);
    }

    [Fact]
    public async Task UpdateImageAsync_PartialUpdate_UpdatesOnlySpecifiedProperties()
    {
        // Arrange
        var imageId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var originalImage = await _context.Images.FindAsync(imageId);
        var originalTitle = originalImage.Title;

        var updatedImage = new Image
        {
            ID = imageId,
            Title = "Only Title Updated",
            Path = originalImage.Path, // Keep original path
            GeneratedWith = originalImage.GeneratedWith // Keep original GeneratedWith
        };

        // Act
        var result = await _repository.UpdateImageAsync(updatedImage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Only Title Updated", result.Title);
        Assert.Equal(originalImage.Path, result.Path); // Should remain unchanged
        Assert.Equal(originalImage.GeneratedWith, result.GeneratedWith); // Should remain unchanged
    }

    [Fact]
    public async Task UpdateImageAsync_NonExistentImage_ReturnsNull()
    {
        // Arrange
        var nonExistentImage = new Image
        {
            ID = Guid.NewGuid(),
            Title = "Non Existent",
            Path = "/path",
            GeneratedWith = "AI"
        };

        // Act
        var result = await _repository.UpdateImageAsync(nonExistentImage);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateImageAsync_NullImage_ReturnsNull()
    {
        // Arrange
        Image nullImage = null;

        // Act & Assert
        var res = await _repository.UpdateImageAsync(nullImage);
        Assert.Null(res);
    }

    [Fact]
    public async Task UpdateImageAsync_TouchMethod_UpdatesLastUpdatedAt()
    {
        // Arrange
        var imageId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var originalImage = await _context.Images.FindAsync(imageId);
        var originalLastUpdatedAt = originalImage.LastUpdatedAt;

        var updatedImage = new Image
        {
            ID = imageId,
            Title = "New Title",
            Path = originalImage.Path,
            GeneratedWith = originalImage.GeneratedWith
        };

        // Wait a bit to ensure time difference
        await Task.Delay(100);

        // Act
        var result = await _repository.UpdateImageAsync(updatedImage);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(originalLastUpdatedAt, result.LastUpdatedAt);
        Assert.True(result.LastUpdatedAt > originalLastUpdatedAt);
    }

    [Fact]
    public async Task IntegrationTest_CompleteImageLifecycle()
    {
        // 1. Add new image
        var newImage = new Image
        {
            ID = Guid.NewGuid(),
            Title = "Integration Test Image",
            Path = "/integration/path",
            GeneratedWith = "Integration AI",
            CreatedAt = DateTime.UtcNow
        };

        var addedImage = await _repository.AddImageAsync(newImage);
        Assert.NotNull(addedImage);

        // 2. Verify it exists
        var exists = await _repository.DoesImageExistAsync(newImage.ID);
        Assert.True(exists);

        // 3. Get it by condition
        var retrievedImage = await _repository.GetImageByConditionAsync(i => i.ID == newImage.ID);
        Assert.NotNull(retrievedImage);

        // 4. Add it to a prompt
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var addedToPrompt = await _repository.AddImageToPromptAsync(promptId, newImage.ID);
        Assert.True(addedToPrompt);

        // 5. Verify it's in the prompt's images
        var promptImages = await _repository.GetImagesByPromptIdAsync(promptId);
        Assert.Contains(promptImages, i => i.ID == newImage.ID);

        // 6. Update the image
        var updatedImageData = new Image
        {
            ID = newImage.ID,
            Title = "Updated Integration Image",
            Path = "/updated/integration/path",
            GeneratedWith = "Updated AI"
        };

        var updatedImage = await _repository.UpdateImageAsync(updatedImageData);
        Assert.NotNull(updatedImage);
        Assert.Equal("Updated Integration Image", updatedImage.Title);

        // 7. Remove from prompt
        var removedFromPrompt = await _repository.RemoveImageFromPromptAsync(promptId, newImage.ID);
        Assert.True(removedFromPrompt);

        // 8. Verify it's removed from prompt
        var promptImagesAfterRemoval = await _repository.GetImagesByPromptIdAsync(promptId);
        Assert.DoesNotContain(promptImagesAfterRemoval, i => i.ID == newImage.ID);

        // 9. Delete the image
        var deleted = await _repository.DeleteImageAsync(newImage.ID);
        Assert.True(deleted);

        // 10. Verify it no longer exists
        exists = await _repository.DoesImageExistAsync(newImage.ID);
        Assert.False(exists);
    }

    [Fact]
    public async Task GetImageByConditionAsync_NullCondition_ThrowsException()
    {
        // Arrange
        Expression<Func<Image, bool>> nullCondition = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetImageByConditionAsync(nullCondition));
    }

    [Fact]
    public async Task AddImageToPromptAsync_DuplicateRelationship_ReturnsTrue()
    {
        // Arrange - Use existing relationship
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var imageId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var result = await _repository.AddImageToPromptAsync(promptId, imageId);

        // Assert - Should return true but relationship already exists
        Assert.True(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

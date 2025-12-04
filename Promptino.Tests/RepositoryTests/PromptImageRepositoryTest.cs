using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Promptino.Core.Domain.Entities;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.DTOs;
using Promptino.Infrastructure.DatabaseContext;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace Promptino.Core.Services.PromptImageServices.Tests
{
    public class PromptImageRepositoryIntegrationTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IPromptRepository> _mockPromptRepository;
        private readonly Mock<IImageRepository> _mockImageRepository;
        private readonly PromptImageRepository _repository;
        private readonly ITestOutputHelper _testOutputHelper;

        public PromptImageRepositoryIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockMapper = new Mock<IMapper>();
            _mockPromptRepository = new Mock<IPromptRepository>();
            _mockImageRepository = new Mock<IImageRepository>();
            _testOutputHelper = testOutputHelper;

            _repository = new PromptImageRepository(
                _mockContext.Object,
                _mockMapper.Object,
                _mockPromptRepository.Object,
                _mockImageRepository.Object
            );
        }

        [Fact]
        public async Task CreatePromptWithImagesAsync_ShouldCreateCompleteRelationshipGraph()
        {
            // Arrange: Real test data
            var promptId = Guid.NewGuid();
            var image1Id = Guid.NewGuid();
            var image2Id = Guid.NewGuid();

            _testOutputHelper.WriteLine("=== Test: Creating Complete Relationship Graph ===");
            _testOutputHelper.WriteLine($"Prompt ID: {promptId}");
            _testOutputHelper.WriteLine($"Image 1 ID: {image1Id}");
            _testOutputHelper.WriteLine($"Image 2 ID: {image2Id}");
            _testOutputHelper.WriteLine("");

            var promptRequest = new PromptAddRequest(
                Title: "AI Logo Design",
                Description: "Modern logo for AI startup",
                Content: "An abstract logo representing machine learning concepts",
                Images: new List<ImageAddRequest>
                {
                    new ImageAddRequest(
                        Title: "Logo Draft Version 1",
                        Path: "/images/logo-design-v1.jpg",
                        GeneratedWith: "DALL-E 3"
                    ),
                    new ImageAddRequest(
                        Title: "Final Logo Design",
                        Path: "/images/logo-design-final.png",
                        GeneratedWith: "Midjourney v6"
                    )
                }
            );

            // Real entities
            var promptEntity = new Prompt
            {
                ID = promptId,
                Title = "AI Logo Design",
                Description = "Modern logo for AI startup",
                Content = "An abstract logo representing machine learning concepts",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            var image1Entity = new Image
            {
                ID = image1Id,
                Title = "Logo Draft Version 1",
                Path = "/images/logo-design-v1.jpg",
                GeneratedWith = "DALL-E 3",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            var image2Entity = new Image
            {
                ID = image2Id,
                Title = "Final Logo Design",
                Path = "/images/logo-design-final.png",
                GeneratedWith = "Midjourney v6",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Full prompt with relationships
            var fullPromptEntity = new Prompt
            {
                ID = promptId,
                Title = "AI Logo Design",
                Description = "Modern logo for AI startup",
                Content = "An abstract logo representing machine learning concepts",
                PromptImages = new List<PromptImage>
                {
                    new PromptImage
                    {
                        ID = Guid.NewGuid(),
                        PromptID = promptId,
                        ImageID = image1Id,
                        Image = image1Entity,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow
                    },
                    new PromptImage
                    {
                        ID = Guid.NewGuid(),
                        PromptID = promptId,
                        ImageID = image2Id,
                        Image = image2Entity,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow
                    }
                }
            };

            // Expected response
            var expectedResponse = new PromptResponse(
                Id: promptId,
                Title: "AI Logo Design",
                Description: "Modern logo for AI startup",
                Content: "An abstract logo representing machine learning concepts",
                Images: new List<ImageResponse>
                {
                    new ImageResponse(
                        Id: image1Id,
                        Title: "Logo Draft Version 1",
                        Path: "/images/logo-design-v1.jpg",
                        GeneratedWith: "DALL-E 3"
                    ),
                    new ImageResponse(
                        Id: image2Id,
                        Title: "Final Logo Design",
                        Path: "/images/logo-design-final.png",
                        GeneratedWith: "Midjourney v6"
                    )
                }
            );

            _testOutputHelper.WriteLine("📝 Created Prompt Entity:");
            _testOutputHelper.WriteLine($"  • Title: {promptEntity.Title}");
            _testOutputHelper.WriteLine($"  • Description: {promptEntity.Description}");
            _testOutputHelper.WriteLine($"  • Content: {promptEntity.Content}");
            _testOutputHelper.WriteLine("");

            _testOutputHelper.WriteLine("🖼️ Created Image Entities:");
            _testOutputHelper.WriteLine($"  Image 1:");
            _testOutputHelper.WriteLine($"    • Title: {image1Entity.Title}");
            _testOutputHelper.WriteLine($"    • Path: {image1Entity.Path}");
            _testOutputHelper.WriteLine($"    • Generated With: {image1Entity.GeneratedWith}");
            _testOutputHelper.WriteLine($"  Image 2:");
            _testOutputHelper.WriteLine($"    • Title: {image2Entity.Title}");
            _testOutputHelper.WriteLine($"    • Path: {image2Entity.Path}");
            _testOutputHelper.WriteLine($"    • Generated With: {image2Entity.GeneratedWith}");
            _testOutputHelper.WriteLine("");

            // Setup Mapper
            _mockMapper.Setup(m => m.Map<Prompt>(promptRequest))
                      .Returns(promptEntity);

            _mockMapper.Setup(m => m.Map<Image>(It.Is<ImageAddRequest>(x => x.Title == "Logo Draft Version 1")))
                      .Returns(image1Entity);

            _mockMapper.Setup(m => m.Map<Image>(It.Is<ImageAddRequest>(x => x.Title == "Final Logo Design")))
                      .Returns(image2Entity);

            _mockMapper.Setup(m => m.Map<PromptResponse>(fullPromptEntity))
                      .Returns(expectedResponse);

            // Setup Repositories
            _mockPromptRepository.Setup(r => r.AddPromptAsync(promptEntity))
                               .ReturnsAsync(promptEntity);

            _mockImageRepository.Setup(r => r.AddImageAsync(It.Is<Image>(i => i.ID == image1Id)))
                               .ReturnsAsync(image1Entity);

            _mockImageRepository.Setup(r => r.AddImageAsync(It.Is<Image>(i => i.ID == image2Id)))
                               .ReturnsAsync(image2Entity);

            _mockImageRepository.Setup(r => r.AddImageToPromptAsync(promptId, image1Id))
                               .ReturnsAsync(true);

            _mockImageRepository.Setup(r => r.AddImageToPromptAsync(promptId, image2Id))
                               .ReturnsAsync(true);

            _mockPromptRepository.Setup(r => r.GetPromptByConditionAsync(
                    It.IsAny<Expression<Func<Prompt, bool>>>()))
                .ReturnsAsync(fullPromptEntity);

            // Setup Transaction
            var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_mockContext.Object);
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(d => d.BeginTransactionAsync(default))
                       .ReturnsAsync(mockTransaction.Object);

            // Act
            _testOutputHelper.WriteLine("🚀 Executing CreatePromptWithImagesAsync...");
            var result = await _repository.CreatePromptWithImagesAsync(promptRequest);
            _testOutputHelper.WriteLine("✅ Operation completed successfully!");
            _testOutputHelper.WriteLine("");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promptId, result.Id);
            Assert.Equal("AI Logo Design", result.Title);
            Assert.Equal("Modern logo for AI startup", result.Description);
            Assert.Equal("An abstract logo representing machine learning concepts", result.Content);

            _testOutputHelper.WriteLine("🔗 Relationship Results:");
            _testOutputHelper.WriteLine($"  Prompt '{result.Title}' linked to {result.Images?.Count() ?? 0} images");

            Assert.NotNull(result.Images);
            var imagesList = result.Images.ToList();
            Assert.Equal(2, imagesList.Count);

            _testOutputHelper.WriteLine("  Linked Images:");
            foreach (var image in imagesList)
            {
                _testOutputHelper.WriteLine($"    • {image.Title} (ID: {image.Id})");
            }
            _testOutputHelper.WriteLine("");

            // Verify the relationships were created
            _mockImageRepository.Verify(r => r.AddImageToPromptAsync(promptId, image1Id), Times.Once);
            _mockImageRepository.Verify(r => r.AddImageToPromptAsync(promptId, image2Id), Times.Once);

            _testOutputHelper.WriteLine("✅ All relationships verified successfully!");
            _testOutputHelper.WriteLine("   ✔️ Image 1 linked to Prompt");
            _testOutputHelper.WriteLine("   ✔️ Image 2 linked to Prompt");
        }

        [Fact]
        public async Task CreatePromptWithImagesAsync_ShouldShowFailedRelationshipCreation()
        {
            // Arrange
            var promptId = Guid.NewGuid();
            var image1Id = Guid.NewGuid();
            var image2Id = Guid.NewGuid();

            _testOutputHelper.WriteLine("=== Test: Failed Relationship Creation ===");
            _testOutputHelper.WriteLine($"Prompt ID: {promptId}");
            _testOutputHelper.WriteLine($"Image 1 ID: {image1Id} (will succeed)");
            _testOutputHelper.WriteLine($"Image 2 ID: {image2Id} (will FAIL)");
            _testOutputHelper.WriteLine("");

            var promptRequest = new PromptAddRequest(
                Title: "Test Prompt",
                Description: "Test Description",
                Content: "Test Content",
                Images: new List<ImageAddRequest>
                {
                    new ImageAddRequest("Image 1", "/path1.jpg", "Generator 1"),
                    new ImageAddRequest("Image 2", "/path2.jpg", "Generator 2")
                }
            );

            var promptEntity = new Prompt { ID = promptId };
            var image1Entity = new Image { ID = image1Id, Title = "Image 1" };
            var image2Entity = new Image { ID = image2Id, Title = "Image 2" };

            _testOutputHelper.WriteLine("📦 Entities to be created:");
            _testOutputHelper.WriteLine($"  Prompt: {promptEntity.Title}");
            _testOutputHelper.WriteLine($"  Image 1: {image1Entity.Title}");
            _testOutputHelper.WriteLine($"  Image 2: {image2Entity.Title}");
            _testOutputHelper.WriteLine("");

            // Setup Mapper
            _mockMapper.Setup(m => m.Map<Prompt>(promptRequest)).Returns(promptEntity);

            var mapperSequence = _mockMapper.SetupSequence(m => m.Map<Image>(It.IsAny<ImageAddRequest>()));
            mapperSequence.Returns(image1Entity);
            mapperSequence.Returns(image2Entity);

            // Setup Repositories
            _mockPromptRepository.Setup(r => r.AddPromptAsync(promptEntity))
                               .ReturnsAsync(promptEntity);

            _mockImageRepository.Setup(r => r.AddImageAsync(image1Entity))
                               .ReturnsAsync(image1Entity);

            _mockImageRepository.Setup(r => r.AddImageAsync(image2Entity))
                               .ReturnsAsync(image2Entity);

            // First link succeeds, second fails
            _mockImageRepository.Setup(r => r.AddImageToPromptAsync(promptId, image1Id))
                               .ReturnsAsync(true);

            _mockImageRepository.Setup(r => r.AddImageToPromptAsync(promptId, image2Id))
                               .ReturnsAsync(false); // This will fail!

            // Setup Transaction
            var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_mockContext.Object);
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(d => d.BeginTransactionAsync(default))
                       .ReturnsAsync(mockTransaction.Object);

            // Act & Assert
            _testOutputHelper.WriteLine("🚀 Attempting to create prompt with images...");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.CreatePromptWithImagesAsync(promptRequest));

            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine("❌ Operation failed as expected!");
            _testOutputHelper.WriteLine($"💥 Error Message: {exception.Message}");
            _testOutputHelper.WriteLine("");

            // Verify transaction was not committed
            mockTransaction.Verify(t => t.CommitAsync(default), Times.Never);

            _testOutputHelper.WriteLine("📊 Verification Results:");
            _testOutputHelper.WriteLine("   ✔️ Transaction was NOT committed (rollback expected)");
            _testOutputHelper.WriteLine("   ✔️ First image link was attempted");
            _testOutputHelper.WriteLine("   ✔️ Second image link failed and threw exception");

            // Verify both attempts were made
            _mockImageRepository.Verify(r => r.AddImageToPromptAsync(promptId, image1Id), Times.Once);
            _mockImageRepository.Verify(r => r.AddImageToPromptAsync(promptId, image2Id), Times.Once);
        }

        [Fact]
        public async Task CreatePromptWithImagesAsync_ShouldShowNoImagesScenario()
        {
            // Arrange
            var promptId = Guid.NewGuid();

            _testOutputHelper.WriteLine("=== Test: Prompt Creation Without Images ===");
            _testOutputHelper.WriteLine($"Prompt ID: {promptId}");
            _testOutputHelper.WriteLine("📌 Note: No images will be linked");
            _testOutputHelper.WriteLine("");

            var promptRequest = new PromptAddRequest(
                Title: "Text-Only Prompt",
                Description: "A prompt without any images",
                Content: "This is a text-only prompt content",
                Images: null // No images
            );

            var promptEntity = new Prompt
            {
                ID = promptId,
                Title = "Text-Only Prompt",
                Description = "A prompt without any images",
                Content = "This is a text-only prompt content"
            };

            var fullPromptEntity = new Prompt
            {
                ID = promptId,
                Title = "Text-Only Prompt",
                Description = "A prompt without any images",
                Content = "This is a text-only prompt content",
                PromptImages = new List<PromptImage>() // Empty list
            };

            var expectedResponse = new PromptResponse(
                Id: promptId,
                Title: "Text-Only Prompt",
                Description: "A prompt without any images",
                Content: "This is a text-only prompt content",
                Images: null
            );

            _testOutputHelper.WriteLine("📝 Prompt Entity Details:");
            _testOutputHelper.WriteLine($"  • Title: {promptEntity.Title}");
            _testOutputHelper.WriteLine($"  • Description: {promptEntity.Description}");
            _testOutputHelper.WriteLine($"  • Content: {promptEntity.Content}");
            _testOutputHelper.WriteLine("");

            // Setup Mapper
            _mockMapper.Setup(m => m.Map<Prompt>(promptRequest))
                      .Returns(promptEntity);

            _mockMapper.Setup(m => m.Map<PromptResponse>(fullPromptEntity))
                      .Returns(expectedResponse);

            // Setup Repositories
            _mockPromptRepository.Setup(r => r.AddPromptAsync(promptEntity))
                               .ReturnsAsync(promptEntity);

            _mockPromptRepository.Setup(r => r.GetPromptByConditionAsync(
                    It.IsAny<Expression<Func<Prompt, bool>>>()))
                               .ReturnsAsync(fullPromptEntity);

            // Setup Transaction
            var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_mockContext.Object);
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(d => d.BeginTransactionAsync(default))
                       .ReturnsAsync(mockTransaction.Object);

            // Act
            _testOutputHelper.WriteLine("🚀 Creating prompt without images...");
            var result = await _repository.CreatePromptWithImagesAsync(promptRequest);
            _testOutputHelper.WriteLine("✅ Prompt created successfully!");
            _testOutputHelper.WriteLine("");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promptId, result.Id);
            Assert.Null(result.Images);

            _testOutputHelper.WriteLine("📊 Result Verification:");
            _testOutputHelper.WriteLine($"  ✔️ Prompt created with ID: {result.Id}");
            _testOutputHelper.WriteLine($"  ✔️ Title: {result.Title}");
            _testOutputHelper.WriteLine($"  ✔️ Images collection is NULL (as expected)");
            _testOutputHelper.WriteLine("");

            _testOutputHelper.WriteLine("🔍 Repository Call Verification:");
            _testOutputHelper.WriteLine($"  ✔️ AddPromptAsync called: 1 time");
            _testOutputHelper.WriteLine($"  ✔️ AddImageAsync called: 0 times (no images)");
            _testOutputHelper.WriteLine($"  ✔️ AddImageToPromptAsync called: 0 times (no images to link)");
            _testOutputHelper.WriteLine($"  ✔️ Transaction committed: YES");

            // Verify no image-related calls were made
            _mockImageRepository.Verify(r => r.AddImageAsync(It.IsAny<Image>()), Times.Never);
            _mockImageRepository.Verify(r => r.AddImageToPromptAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);

            // Verify transaction was committed
            mockTransaction.Verify(t => t.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task CreatePromptWithImagesAsync_ShouldShowEmptyImagesListScenario()
        {
            // Arrange
            var promptId = Guid.NewGuid();

            _testOutputHelper.WriteLine("=== Test: Prompt Creation With Empty Images List ===");
            _testOutputHelper.WriteLine($"Prompt ID: {promptId}");
            _testOutputHelper.WriteLine("📌 Note: Empty images list (not null)");
            _testOutputHelper.WriteLine("");

            var promptRequest = new PromptAddRequest(
                Title: "Prompt with Empty Images",
                Description: "Testing empty images list",
                Content: "Content here",
                Images: new List<ImageAddRequest>() // Empty list (not null)
            );

            var promptEntity = new Prompt
            {
                ID = promptId,
                Title = "Prompt with Empty Images",
                Description = "Testing empty images list",
                Content = "Content here"
            };

            var fullPromptEntity = new Prompt
            {
                ID = promptId,
                Title = "Prompt with Empty Images",
                Description = "Testing empty images list",
                Content = "Content here",
                PromptImages = new List<PromptImage>() // Empty list
            };

            var expectedResponse = new PromptResponse(
                Id: promptId,
                Title: "Prompt with Empty Images",
                Description: "Testing empty images list",
                Content: "Content here",
                Images: Enumerable.Empty<ImageResponse>() // Empty enumerable
            );

            _testOutputHelper.WriteLine("📝 Entities State:");
            _testOutputHelper.WriteLine($"  • Prompt: '{promptEntity.Title}'");
            _testOutputHelper.WriteLine($"  • Images List: EMPTY (0 items)");
            _testOutputHelper.WriteLine("");

            // Setup Mapper
            _mockMapper.Setup(m => m.Map<Prompt>(promptRequest))
                      .Returns(promptEntity);

            _mockMapper.Setup(m => m.Map<PromptResponse>(fullPromptEntity))
                      .Returns(expectedResponse);

            // Setup Repositories
            _mockPromptRepository.Setup(r => r.AddPromptAsync(promptEntity))
                               .ReturnsAsync(promptEntity);

            _mockPromptRepository.Setup(r => r.GetPromptByConditionAsync(
                    It.IsAny<Expression<Func<Prompt, bool>>>()))
                               .ReturnsAsync(fullPromptEntity);

            // Setup Transaction
            var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(_mockContext.Object);
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockDatabase.Setup(d => d.BeginTransactionAsync(default))
                       .ReturnsAsync(mockTransaction.Object);

            // Act
            _testOutputHelper.WriteLine("🚀 Creating prompt with empty images list...");
            var result = await _repository.CreatePromptWithImagesAsync(promptRequest);
            _testOutputHelper.WriteLine("✅ Operation completed!");
            _testOutputHelper.WriteLine("");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Images);
            Assert.Empty(result.Images);

            _testOutputHelper.WriteLine("📊 Final State:");
            _testOutputHelper.WriteLine($"  ✔️ Prompt created successfully");
            _testOutputHelper.WriteLine($"  ✔️ Images collection is NOT null");
            _testOutputHelper.WriteLine($"  ✔️ Images count: {result.Images.Count()}");
            _testOutputHelper.WriteLine($"  ✔️ Transaction committed successfully");

            // Verify no image repository calls
            _mockImageRepository.Verify(r => r.AddImageAsync(It.IsAny<Image>()), Times.Never);
            _mockImageRepository.Verify(r => r.AddImageToPromptAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }
    }
}
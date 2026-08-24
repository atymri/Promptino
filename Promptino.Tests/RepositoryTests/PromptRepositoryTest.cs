using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using Promptino.Infrastructure.DatabaseContext;
using Promptino.Infrastructure.Repositories;
using System.Linq.Expressions;

namespace Promptino.Infrastructure.Tests.Repositories;

// NOTE: AI DRIVEN DEVELOPED!!
public class PromptRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PromptRepository _repository;
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ApplicationUser _testUser;

    public PromptRepositoryTests()
    {
        // Create in-memory database options
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        // Create context and repository
        _context = new ApplicationDbContext(_options);
        _repository = new PromptRepository(_context);

        // Create test user
        _testUser = new ApplicationUser
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Seed initial data
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        // Add test user
        _context.Users.Add(_testUser);

        // Add test images
        var images = new List<Image>
            {
                new Image
                {
                    ID = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Title = "Image 1",
                    Path = "/path1",
                    GeneratedWith = "AI 1",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Image
                {
                    ID = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Title = "Image 2",
                    Path = "/path2",
                    GeneratedWith = "AI 2",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Image
                {
                    ID = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Title = "Image 3",
                    Path = "/path3",
                    GeneratedWith = "AI 3",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                }
            };

        // Add test prompts (all owned by the test user)
        var prompts = new List<Prompt>
            {
                new Prompt
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    UserID = _testUser.Id,
                    Title = "Prompt 1",
                    Description = "Description for prompt 1",
                    Content = "Content for prompt 1",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Prompt
                {
                    ID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    UserID = _testUser.Id,
                    Title = "Prompt 2",
                    Description = "Description for prompt 2",
                    Content = "Content for prompt 2",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new Prompt
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    UserID = _testUser.Id,
                    Title = "Another Prompt",
                    Description = "Another description",
                    Content = "Another content",
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
                }
            };

        // Add saved prompts (replaces favorites)
        var savedPrompts = new List<SavedPrompt>
            {
                new SavedPrompt
                {
                    ID = Guid.NewGuid(),
                    UserID = _testUser.Id,
                    PromptID = prompts[0].ID,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new SavedPrompt
                {
                    ID = Guid.NewGuid(),
                    UserID = _testUser.Id,
                    PromptID = prompts[1].ID,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                }
            };

        _context.Images.AddRange(images);
        _context.Prompts.AddRange(prompts);
        _context.PromptImages.AddRange(promptImages);
        _context.SavedPrompts.AddRange(savedPrompts);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddPromptAsync_ValidPrompt_ReturnsAddedPrompt()
    {
        // Arrange
        var newPrompt = new Prompt
        {
            ID = Guid.NewGuid(),
            UserID = _testUser.Id,
            Title = "New Prompt",
            Description = "New Description",
            Content = "New Content",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddPromptAsync(newPrompt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newPrompt.ID, result.ID);
        Assert.Equal(newPrompt.Title, result.Title);
        Assert.Equal(newPrompt.Description, result.Description);
        Assert.Equal(newPrompt.Content, result.Content);

        // Verify it was saved to database
        var savedPrompt = await _context.Prompts.FindAsync(newPrompt.ID);
        Assert.NotNull(savedPrompt);
    }

    [Fact]
    public async Task AddPromptAsync_NullPrompt_ThrowsException()
    {
        // Arrange
        Prompt nullPrompt = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddPromptAsync(nullPrompt));
    }

    [Fact]
    public async Task GetPromptsByOwnerAsync_UserWithPrompts_ReturnsOnlyOwned()
    {
        // Act
        var result = await _repository.GetPromptsByOwnerAsync(_testUser.Id);

        // Assert - all 3 seeded prompts are owned by the test user
        Assert.Equal(3, result.Count());
        Assert.All(result, p => Assert.Equal(_testUser.Id, p.UserID));
    }

    [Fact]
    public async Task GetPromptsByOwnerAsync_UserWithoutPrompts_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetPromptsByOwnerAsync(Guid.NewGuid());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPromptOwnerIdAsync_ExistingPrompt_ReturnsOwnerId()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var ownerId = await _repository.GetPromptOwnerIdAsync(promptId);

        // Assert
        Assert.Equal(_testUser.Id, ownerId);
    }

    [Fact]
    public async Task GetPromptOwnerIdAsync_NonExistentPrompt_ReturnsNull()
    {
        // Act
        var ownerId = await _repository.GetPromptOwnerIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(ownerId);
    }

    [Fact]
    public async Task DeletePromptAsync_RemovesCommentsReactionsAndSaves()
    {
        // Arrange - attach children to prompt 1
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var otherUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "other@example.com", Email = "other@example.com" };
        _context.Users.Add(otherUser);

        _context.Comments.Add(new Comment { ID = Guid.NewGuid(), UserID = otherUser.Id, PromptID = promptId, Content = "nice" });
        _context.PromptReactions.Add(new PromptReaction { ID = Guid.NewGuid(), UserID = otherUser.Id, PromptID = promptId, Type = ReactionType.Like });
        await _context.SaveChangesAsync();

        var commentsBefore = await _context.Comments.CountAsync(c => c.PromptID == promptId);
        var reactionsBefore = await _context.PromptReactions.CountAsync(r => r.PromptID == promptId);
        Assert.Equal(1, commentsBefore);
        Assert.Equal(1, reactionsBefore);

        // Act
        var result = await _repository.DeletePromptAsync(promptId);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Prompts.FindAsync(promptId));

        // InMemory does not enforce DB cascades — the repository must clean up explicitly
        Assert.Empty(await _context.Comments.Where(c => c.PromptID == promptId).ToListAsync());
        Assert.Empty(await _context.PromptReactions.Where(r => r.PromptID == promptId).ToListAsync());
    }

    [Fact]
    public async Task DeletePromptAsync_ExistingPrompt_ReturnsTrue()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Verify prompt exists first
        var promptExists = await _context.Prompts.AnyAsync(p => p.ID == promptId);
        Assert.True(promptExists);

        // Act
        var result = await _repository.DeletePromptAsync(promptId);

        // Assert
        Assert.True(result);

        // Verify prompt was deleted
        var deletedPrompt = await _context.Prompts.FindAsync(promptId);
        Assert.Null(deletedPrompt);
    }

    [Fact]
    public async Task DeletePromptAsync_NonExistentPrompt_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeletePromptAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DoesPromptExistAsync_ExistingPrompt_ReturnsTrue()
    {
        // Arrange
        var existingPromptId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _repository.DoesPromptExistAsync(existingPromptId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DoesPromptExistAsync_NonExistentPrompt_ReturnsFalse()
    {
        // Arrange
        var nonExistentPromptId = Guid.NewGuid();

        // Act
        var result = await _repository.DoesPromptExistAsync(nonExistentPromptId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPromptByConditionAsync_ValidCondition_ReturnsPrompt()
    {
        // Arrange
        Expression<Func<Prompt, bool>> condition = p => p.Title == "Prompt 1";

        // Act
        var result = await _repository.GetPromptByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Prompt 1", result.Title);
        Assert.NotNull(result.PromptImages);
        Assert.NotEmpty(result.PromptImages);
        Assert.All(result.PromptImages, pi => Assert.NotNull(pi.Image));
    }

    [Fact]
    public async Task GetPromptByConditionAsync_WithImageInclusion_ReturnsPromptWithImages()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Expression<Func<Prompt, bool>> condition = p => p.ID == promptId;

        // Act
        var result = await _repository.GetPromptByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(promptId, result.ID);
        Assert.NotNull(result.PromptImages);
        Assert.Equal(2, result.PromptImages.Count); // Prompt 1 has 2 images
        Assert.All(result.PromptImages, pi => Assert.NotNull(pi.Image));
    }

    [Fact]
    public async Task GetPromptByConditionAsync_NoMatch_ReturnsNull()
    {
        // Arrange
        Expression<Func<Prompt, bool>> condition = p => p.Title == "Non Existent Prompt";

        // Act
        var result = await _repository.GetPromptByConditionAsync(condition);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPromptByConditionAsync_NullCondition_ThrowsException()
    {
        // Arrange
        Expression<Func<Prompt, bool>> nullCondition = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetPromptByConditionAsync(nullCondition));
    }

    [Fact]
    public async Task GetPromptsAsync_ReturnsAllPromptsWithImages()
    {
        // Act
        var result = await _repository.GetPromptsPagedAsync(1, PaginationDefaults.MaxPageSize);

        // Assert
        Assert.Equal(3, result.TotalCount); // Should have 3 seeded prompts

        // Verify all prompts have PromptImages loaded
        Assert.All(result.Items, prompt => Assert.NotNull(prompt.PromptImages));

        // Verify Images are loaded in PromptImages
        Assert.All(result.Items.SelectMany(p => p.PromptImages),
            pi => Assert.NotNull(pi.Image));
    }

    [Fact]
    public async Task GetPromptsByConditionAsync_ValidCondition_ReturnsFilteredPrompts()
    {
        // Arrange
        Expression<Func<Prompt, bool>> condition = p => p.Title.Contains("Prompt");

        // Act
        var result = await _repository.GetPromptsByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count()); // All 3 prompts have "Prompt" in title
        Assert.All(result, prompt => Assert.Contains("Prompt", prompt.Title));
        Assert.All(result, prompt => Assert.NotNull(prompt.PromptImages));
    }

    [Fact]
    public async Task GetPromptsByConditionAsync_MultipleConditions_ReturnsFilteredPrompts()
    {
        // Arrange
        Expression<Func<Prompt, bool>> condition = p =>
            p.Title == "Prompt 1" || p.Title == "Prompt 2";

        // Act
        var result = await _repository.GetPromptsByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, prompt =>
            Assert.True(prompt.Title == "Prompt 1" || prompt.Title == "Prompt 2"));
    }

    [Fact]
    public async Task GetPromptsByConditionAsync_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        Expression<Func<Prompt, bool>> condition = p => p.Title == "Non Existent";

        // Act
        var result = await _repository.GetPromptsByConditionAsync(condition);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchPromptAsync_KeywordInTitle_ReturnsMatchingPrompts()
    {
        // Arrange
        var keyword = "Prompt";

        // Act
        var result = await _repository.SearchPromptPagedAsync(keyword, 1, PaginationDefaults.MaxPageSize);

        // Assert
        Assert.Equal(3, result.TotalCount); // All prompts have "Prompt" in title
        Assert.All(result.Items, prompt => Assert.Contains(keyword, prompt.Title));
        Assert.All(result.Items, prompt => Assert.NotNull(prompt.PromptImages));
    }

    [Fact]
    public async Task SearchPromptAsync_KeywordInDescription_ReturnsMatchingPrompts()
    {
        // Arrange
        var keyword = "description";

        // Act
        var result = await _repository.SearchPromptPagedAsync(keyword, 1, PaginationDefaults.MaxPageSize);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Items, prompt =>
            Assert.Contains(keyword, prompt.Description, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchPromptAsync_SpecificKeyword_ReturnsSpecificPrompt()
    {
        // Arrange
        var keyword = "Another";

        // Act
        var result = await _repository.SearchPromptPagedAsync(keyword, 1, PaginationDefaults.MaxPageSize);

        // Assert
        Assert.Single(result.Items);
        Assert.Contains(keyword, result.Items.First().Title);
    }

    [Fact]
    public async Task SearchPromptAsync_KeywordNotFound_ReturnsEmptyList()
    {
        // Arrange
        var keyword = "NonexistentKeyword";

        // Act
        var result = await _repository.SearchPromptPagedAsync(keyword, 1, PaginationDefaults.MaxPageSize);

        // Assert
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task SearchPromptAsync_EmptyKeyword_ReturnsAllPrompts()
    {
        // Arrange
        var keyword = "";

        // Act
        var result = await _repository.SearchPromptPagedAsync(keyword, 1, PaginationDefaults.MaxPageSize);

        // Assert
        Assert.Equal(3, result.TotalCount); // Should return all prompts
    }

    [Fact]
    public async Task SearchPromptAsync_NullKeyword_ReturnsEmpty()
    {
        // Arrange
        string keyword = null;

        // Act & Assert
        var res = await _repository.SearchPromptPagedAsync(keyword, 1, PaginationDefaults.MaxPageSize);
        Assert.NotNull(res);
        Assert.Empty(res.Items);
        Assert.Equal(0, res.TotalCount);
    }

    [Fact]
    public async Task UpdatePromptAsync_ExistingPrompt_ReturnsUpdatedPrompt()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var updatedPrompt = new Prompt
        {
            ID = promptId,
            Title = "Updated Title",
            Description = "Updated Description",
            Content = "Updated Content"
        };

        // Act
        var result = await _repository.UpdatePromptAsync(updatedPrompt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal("Updated Content", result.Content);
        Assert.NotNull(result.LastUpdatedAt);

        // Verify changes were saved
        var savedPrompt = await _context.Prompts.FindAsync(promptId);
        Assert.Equal("Updated Title", savedPrompt.Title);
        Assert.Equal("Updated Description", savedPrompt.Description);
        Assert.Equal("Updated Content", savedPrompt.Content);
    }

    [Fact]
    public async Task UpdatePromptAsync_PreservesPromptImages()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Get original prompt images count
        var originalPrompt = await _context.Prompts
            .Include(p => p.PromptImages)
            .FirstOrDefaultAsync(p => p.ID == promptId);
        var originalImageCount = originalPrompt.PromptImages.Count;
        Assert.Equal(2, originalImageCount);

        var updatedPrompt = new Prompt
        {
            ID = promptId,
            Title = "Updated Title",
            Description = "Updated Description",
            Content = "Updated Content"
        };

        // Act
        var result = await _repository.UpdatePromptAsync(updatedPrompt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.NotNull(result.PromptImages);
        Assert.Equal(originalImageCount, result.PromptImages.Count); // Images should be preserved
    }

    [Fact]
    public async Task UpdatePromptAsync_NonExistentPrompt_ReturnsNull()
    {
        // Arrange
        var nonExistentPrompt = new Prompt
        {
            ID = Guid.NewGuid(),
            Title = "Non Existent",
            Description = "Description",
            Content = "Content"
        };

        // Act
        var result = await _repository.UpdatePromptAsync(nonExistentPrompt);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePromptAsync_TouchMethod_UpdatesLastUpdatedAt()
    {
        // Arrange
        var promptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var originalPrompt = await _context.Prompts.FindAsync(promptId);
        var originalLastUpdatedAt = originalPrompt.LastUpdatedAt;

        var updatedPrompt = new Prompt
        {
            ID = promptId,
            Title = "New Title",
            Description = originalPrompt.Description,
            Content = originalPrompt.Content
        };

        // Wait a bit to ensure time difference
        await Task.Delay(100);

        // Act
        var result = await _repository.UpdatePromptAsync(updatedPrompt);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(originalLastUpdatedAt, result.LastUpdatedAt);
        Assert.True(result.LastUpdatedAt > originalLastUpdatedAt);
    }

    [Fact]
    public async Task IntegrationTest_CompletePromptLifecycle()
    {
        // 1. Add new prompt owned by test user
        var newPrompt = new Prompt
        {
            ID = Guid.NewGuid(),
            UserID = _testUser.Id,
            Title = "Integration Test Prompt",
            Description = "Integration Description",
            Content = "Integration Content",
            CreatedAt = DateTime.UtcNow
        };

        var addedPrompt = await _repository.AddPromptAsync(newPrompt);
        Assert.NotNull(addedPrompt);

        // 2. Verify it exists and owner is correct
        var exists = await _repository.DoesPromptExistAsync(newPrompt.ID);
        Assert.True(exists);
        var ownerId = await _repository.GetPromptOwnerIdAsync(newPrompt.ID);
        Assert.Equal(_testUser.Id, ownerId);

        // 3. Get it by owner
        var owned = await _repository.GetPromptsByOwnerAsync(_testUser.Id);
        Assert.Contains(owned, p => p.ID == newPrompt.ID);

        // 4. Get it by condition
        var retrievedPrompt = await _repository.GetPromptByConditionAsync(p => p.ID == newPrompt.ID);
        Assert.NotNull(retrievedPrompt);

        // 5. Search for it
        var searchResults = await _repository.SearchPromptPagedAsync("Integration", 1, PaginationDefaults.MaxPageSize);
        Assert.Contains(searchResults.Items, p => p.ID == newPrompt.ID);

        // 6. Update the prompt
        var updatedPromptData = new Prompt
        {
            ID = newPrompt.ID,
            Title = "Updated Integration Prompt",
            Description = "Updated Integration Description",
            Content = "Updated Integration Content"
        };

        var updatedPrompt = await _repository.UpdatePromptAsync(updatedPromptData);
        Assert.NotNull(updatedPrompt);
        Assert.Equal("Updated Integration Prompt", updatedPrompt.Title);

        // 7. Delete the prompt
        var deleted = await _repository.DeletePromptAsync(newPrompt.ID);
        Assert.True(deleted);

        // 8. Verify it no longer exists
        exists = await _repository.DoesPromptExistAsync(newPrompt.ID);
        Assert.False(exists);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

namespace Promptino.Core.DTOs;

public record CategoryAddRequest(string Title, string Description);

public record CategoryUpdateRequest(Guid CategoryID, string Title, string Description);

public record DeletePromptFromCategoryRequest(Guid CategoryID, Guid PromptID);

public record AddPromptToCategoryRequest(Guid CategoryID, Guid PromptID);

public record CategoryResponse(Guid CategoryID, string Title, string Description);

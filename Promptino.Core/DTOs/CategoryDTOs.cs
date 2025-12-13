
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Promptino.Core.DTOs;

public record CategoryAddRequest(string Title, string Description)
{
    public CategoryAddRequest() : this(default, default)
    {
        
    }
};

public record CategoryUpdateRequest(Guid CategoryID, string Title, string Description)
{
    public CategoryUpdateRequest() : this(default, default, default)
    {
        
    }
};

public record DeletePromptFromCategoryRequest(Guid CategoryID, Guid PromptID)
{
    public DeletePromptFromCategoryRequest() : this(default, default)
    {
        
    }
};

public record AddPromptToCategoryRequest(Guid CategoryID, Guid PromptID)
{
    public AddPromptToCategoryRequest() : this(default, default)
    {
        
    }
};

public record CategoryResponse(Guid CategoryID, string Title, string Description)
{
    public CategoryResponse() : this(default, default, default)
    {
        
    }
};

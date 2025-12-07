using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using System.Linq.Expressions;

namespace Promptino.API.Controllers;

public class PromptsController : BaseController
{
    private readonly IPromptGetterService _promptGetterService;
    private readonly IPromptAdderService _promptAdderService;
    private readonly IPromptDeleterService _promptDeleterService;
    public PromptsController(IPromptGetterService getterService,
        IPromptDeleterService deleterService, IPromptAdderService promptAdderService)
    {
        _promptGetterService = getterService;
        _promptAdderService = promptAdderService;
        _promptDeleterService = deleterService;

    }

    [HttpGet]
    public async Task<IActionResult> GetPrompts()
    {
        var prompts = await _promptGetterService.GetAllPromptsAsync();
        return Ok(prompts);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchPrompt([FromQuery] string keyword)
    {
        var prompts = await _promptGetterService.SearchPromptsAsync(keyword);
        return Ok(prompts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PromptResponse>> GetPromptById([FromRoute] Guid id)
    {
        var prompt = await _promptGetterService.GetPromptByConditionAsync(p => p.Id == id);

        if (prompt is null)
            return Problem("پرامپت مورد نظر یافت نشد",
                statusCode: StatusCodes.Status404NotFound,
                title: "خطای یافت نشد");

        return Ok(prompt);
    }

    [HttpGet("favorites/{userId:guid}")]
    public async Task<IActionResult> GetFavoritePrompts([FromRoute] Guid userId)
    {
        var favoritePrompts = await _promptGetterService.GetFavoritePromptsAsync(userId);
        return Ok(favoritePrompts);
    }

    [HttpPost("prompts/favorites")]
    public async Task<ActionResult> AddToFavorites([FromBody] FavoritePromptAddRequest request)
    {
        var result = await _promptAdderService.AddToFavoritesAsync(request);
        return Ok(result);
    }

    [HttpDelete("prompts/favorites/{userId:guid}/{promptId:guid}")]
    public async Task<ActionResult> RemoveFromFavorites(Guid userId, Guid promptId)
    {
        var result = await _promptDeleterService.RemoveFromFavoritesAsync(userId, promptId);
        return Ok(result);
    }
}
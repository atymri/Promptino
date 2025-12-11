using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.ImageServiceContracts;

namespace Promptino.API.Controllers;

public class PromptsController : BaseController
{
    private readonly IPromptGetterService _promptGetterService;
    private readonly IPromptAdderService _promptAdderService;
    private readonly IPromptDeleterService _promptDeleterService;

    public PromptsController(
        IPromptGetterService getterService,
        IPromptDeleterService deleterService,
        IPromptAdderService promptAdderService)
    {
        _promptGetterService = getterService;
        _promptAdderService = promptAdderService;
        _promptDeleterService = deleterService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrompts()
    {
        var prompts = await _promptGetterService.GetAllPromptsAsync();
        return Ok(prompts);
    }


    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<PromptResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchPrompt([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Problem("کلید واژه ارسال نشده است.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "درخواست نامعتبر");

        var prompts = await _promptGetterService.SearchPromptsAsync(keyword);
        return Ok(prompts);
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(IEnumerable<PromptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFavoritePrompts([FromRoute] Guid userId)
    {
        var favoritePrompts = await _promptGetterService.GetFavoritePromptsAsync(userId);
        return Ok(favoritePrompts);
    }


    [HttpPost("favorites")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddToFavorites([FromBody] FavoritePromptAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _promptAdderService.AddToFavoritesAsync(request);
        return Ok(result);
    }


    [HttpDelete("favorites/{userId:guid}/{promptId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RemoveFromFavorites(Guid userId, Guid promptId)
    {
        var result = await _promptDeleterService.RemoveFromFavoritesAsync(userId, promptId);
        return Ok(result);
    }

    [HttpGet("favrites/{promptId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetCountOfFavorites(Guid promptId)
    {
        var res = await _promptGetterService.GetFavoritePromptsAsync(promptId);
        return Ok(res.Count());
    }
}

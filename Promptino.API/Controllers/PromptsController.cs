using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.ImageServiceContracts;

namespace Promptino.API.Controllers;

public class PromptsController : BaseController
{
    private readonly IPromptGetterService _promptGetterService;
    private readonly IPromptAdderService _promptAdderService;
    private readonly IPromptUpdaterService _promptUpdaterService;
    private readonly IPromptDeleterService _promptDeleterService;

    public PromptsController(
        IPromptGetterService getterService,
        IPromptAdderService adderService,
        IPromptUpdaterService updaterService,
        IPromptDeleterService deleterService)
    {
        _promptGetterService = getterService;
        _promptAdderService = adderService;
        _promptUpdaterService = updaterService;
        _promptDeleterService = deleterService;
    }

    // ─────────────────────────────── Public ───────────────────────────────

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
            return Problem(
                "کلید واژه ارسال نشده است.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "درخواست نامعتبر");

        var prompts = await _promptGetterService.SearchPromptsAsync(keyword);
        return Ok(prompts);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptResponse>> GetPromptById(Guid id)
    {
        var prompt = await _promptGetterService.GetPromptByConditionAsync(p => p.Id == id);

        if (prompt is null)
            return Problem(
                "پرامپت مورد نظر یافت نشد",
                statusCode: StatusCodes.Status404NotFound,
                title: "خطای یافت نشد");

        return Ok(prompt);
    }

    // ─────────────────────────────── Favorites ───────────────────────────────

    [HttpGet("favorites/{userId:guid}")]
    public async Task<IActionResult> GetFavoritePrompts(Guid userId)
    {
        var favorites = await _promptGetterService.GetFavoritePromptsAsync(userId);
        return Ok(favorites);
    }

    [HttpPost("favorites")]
    public async Task<ActionResult> AddToFavorites([FromBody] FavoritePromptAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _promptAdderService.AddToFavoritesAsync(request);
        return Ok(result);
    }

    [HttpDelete("favorites/{userId:guid}/{promptId:guid}")]
    public async Task<ActionResult> RemoveFromFavorites(Guid userId, Guid promptId)
    {
        var result = await _promptDeleterService.RemoveFromFavoritesAsync(userId, promptId);
        return Ok(result);
    }

    [HttpGet("favorites/count/{promptId:guid}")]
    public async Task<ActionResult<int>> GetCountOfFavorites(Guid promptId)
    {
        var res = await _promptGetterService.GetFavoritePromptsAsync(promptId);
        return Ok(res.Count());
    }

    // ─────────────────────────────── Admin (Prompt Management) ───────────────────────────────

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PromptResponse>> CreatePrompt([FromBody] PromptAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _promptAdderService.CreatePromptAsync(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PromptResponse>> UpdatePrompt([FromBody] PromptUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _promptUpdaterService.UpdatePromptAsync(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> DeletePrompt(Guid id)
    {
        var result = await _promptDeleterService.DeletePromptAsync(id);
        return Ok(result);
    }
}

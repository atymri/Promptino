using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.CommentServiceContracts;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using Promptino.Core.ServiceContracts.PromptReactionServiceContracts;
using Promptino.Core.ServiceContracts.SavedPromptServiceContracts;

namespace Promptino.API.Controllers;

public class PromptsController : BaseController
{
    private readonly IPromptGetterService _promptGetterService;
    private readonly IPromptAdderService _promptAdderService;
    private readonly IPromptUpdaterService _promptUpdaterService;
    private readonly IPromptDeleterService _promptDeleterService;
    private readonly ISavedPromptAdderService _savedAdderService;
    private readonly ISavedPromptDeleterService _savedDeleterService;
    private readonly ISavedPromptGetterService _savedGetterService;
    private readonly IPromptReactionSetterService _reactionSetterService;
    private readonly IPromptReactionRemoverService _reactionRemoverService;
    private readonly IPromptReactionGetterService _reactionGetterService;

    public PromptsController(
        IPromptGetterService getterService,
        IPromptAdderService adderService,
        IPromptUpdaterService updaterService,
        IPromptDeleterService deleterService,
        ISavedPromptAdderService savedAdderService,
        ISavedPromptDeleterService savedDeleterService,
        ISavedPromptGetterService savedGetterService,
        IPromptReactionSetterService reactionSetterService,
        IPromptReactionRemoverService reactionRemoverService,
        IPromptReactionGetterService reactionGetterService)
    {
        _promptGetterService = getterService;
        _promptAdderService = adderService;
        _promptUpdaterService = updaterService;
        _promptDeleterService = deleterService;
        _savedAdderService = savedAdderService;
        _savedDeleterService = savedDeleterService;
        _savedGetterService = savedGetterService;
        _reactionSetterService = reactionSetterService;
        _reactionRemoverService = reactionRemoverService;
        _reactionGetterService = reactionGetterService;
    }

    // ─────────────────────────────── Public ───────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PromptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrompts([FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var prompts = await _promptGetterService.GetAllPromptsAsync(page, pageSize);
        return Ok(prompts);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<PromptResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchPrompt([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Problem(
                "کلید واژه ارسال نشده است.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "درخواست نامعتبر");

        var prompts = await _promptGetterService.SearchPromptsAsync(keyword, page, pageSize);
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

    // ─────────────────────────────── Prompt Management (Owner or Admin) ───────────────────────────────

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PromptResponse>> CreatePrompt([FromBody] PromptAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var result = await _promptAdderService.CreatePromptAsync(request, userId.Value);
        return Ok(result);
    }

    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PromptResponse>> UpdatePrompt([FromBody] PromptUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var result = await _promptUpdaterService.UpdatePromptAsync(request, userId.Value, User.IsInRole("Admin"));
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<bool>> DeletePrompt(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var result = await _promptDeleterService.DeletePromptAsync(id, userId.Value, User.IsInRole("Admin"));
        return Ok(result);
    }

    [Authorize]
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<PromptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPrompts()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var prompts = await _promptGetterService.GetPromptsByOwnerAsync(userId.Value);
        return Ok(prompts);
    }

    // ─────────────────────────────── Saves ───────────────────────────────

    [Authorize]
    [HttpGet("saves")]
    [ProducesResponseType(typeof(IEnumerable<SavedWithDetailsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavedPrompts()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var saves = await _savedGetterService.GetSavedPromptsAsync(userId.Value);
        return Ok(saves);
    }

    [Authorize]
    [HttpPost("saves")]
    [ProducesResponseType(typeof(SavedWithDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> SavePrompt([FromBody] SavedPromptAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var result = await _savedAdderService.SaveAsync(userId.Value, request.PromptID);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("saves/{promptId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UnsavePrompt(Guid promptId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var result = await _savedDeleterService.UnsaveAsync(userId.Value, promptId);
        return Ok(result);
    }

    [HttpGet("saves/count/{promptId:guid}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetSavedCount(Guid promptId)
    {
        var count = await _savedGetterService.GetSavedCountAsync(promptId);
        return Ok(count);
    }

    [Authorize]
    [HttpGet("saves/{promptId:guid}/status")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsSaved(Guid promptId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        return Ok(await _savedGetterService.IsSavedAsync(userId.Value, promptId));
    }

    // ─────────────────────────────── Reactions (Like / Dislike) ───────────────────────────────

    [Authorize]
    [HttpPut("{promptId:guid}/reaction")]
    [ProducesResponseType(typeof(ReactionStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReactionStateResponse>> SetReaction(Guid promptId, [FromBody] ReactionAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var state = await _reactionSetterService.SetReactionAsync(userId.Value, promptId, request.Type);
        return Ok(state);
    }

    [Authorize]
    [HttpDelete("{promptId:guid}/reaction")]
    [ProducesResponseType(typeof(ReactionStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReactionStateResponse>> RemoveReaction(Guid promptId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var state = await _reactionRemoverService.RemoveReactionAsync(userId.Value, promptId);
        return Ok(state);
    }

    [HttpGet("{promptId:guid}/reaction/state")]
    [ProducesResponseType(typeof(ReactionStateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReactionStateResponse>> GetReactionState(Guid promptId)
    {
        var userId = GetCurrentUserId() ?? Guid.Empty;
        var state = await _reactionGetterService.GetStateAsync(userId, promptId);
        return Ok(state);
    }
}

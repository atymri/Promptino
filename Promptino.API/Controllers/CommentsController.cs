using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.CommentServiceContracts;

namespace Promptino.API.Controllers;

[Route("api/prompts/{promptId:guid}/comments")]
public class CommentsController : BaseController
{
    private readonly ICommentAdderService _commentAdderService;
    private readonly ICommentDeleterService _commentDeleterService;
    private readonly ICommentGetterService _commentGetterService;
    private readonly ICommentLikeSetterService _likeSetterService;
    private readonly ICommentLikeRemoverService _likeRemoverService;

    public CommentsController(
        ICommentAdderService commentAdderService,
        ICommentDeleterService commentDeleterService,
        ICommentGetterService commentGetterService,
        ICommentLikeSetterService likeSetterService,
        ICommentLikeRemoverService likeRemoverService)
    {
        _commentAdderService = commentAdderService;
        _commentDeleterService = commentDeleterService;
        _commentGetterService = commentGetterService;
        _likeSetterService = likeSetterService;
        _likeRemoverService = likeRemoverService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CommentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(Guid promptId, [FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var comments = await _commentGetterService.GetCommentsForPromptAsync(promptId, GetCurrentUserId(), page, pageSize);
        return Ok(comments);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentResponse>> AddComment(Guid promptId, [FromBody] CommentAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var requestWithRoutePromptId = request with { PromptID = promptId };
        var result = await _commentAdderService.AddCommentAsync(userId.Value, requestWithRoutePromptId);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{commentId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteComment(Guid commentId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var result = await _commentDeleterService.DeleteCommentAsync(commentId, userId.Value, User.IsInRole("Admin"));
        return Ok(result);
    }

    // ─────────────────────────────── Comment Likes ───────────────────────────────

    [Authorize]
    [HttpPut("{commentId:guid}/like")]
    [ProducesResponseType(typeof(CommentLikeStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentLikeStateResponse>> ToggleLike(Guid promptId, Guid commentId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var state = await _likeSetterService.ToggleLikeAsync(userId.Value, promptId, commentId);
        return Ok(state);
    }

    [Authorize]
    [HttpDelete("{commentId:guid}/like")]
    [ProducesResponseType(typeof(CommentLikeStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentLikeStateResponse>> RemoveLike(Guid promptId, Guid commentId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var state = await _likeRemoverService.RemoveLikeAsync(userId.Value, promptId, commentId);
        return Ok(state);
    }
}

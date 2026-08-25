using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.ReportServiceContracts;

namespace Promptino.API.Controllers;

[Route("api/reports")]
public class ReportsController : BaseController
{
    private readonly IPromptReportAdderService _reportAdderService;
    private readonly IPromptReportGetterService _reportGetterService;
    private readonly IPromptReportResolverService _reportResolverService;

    public ReportsController(
        IPromptReportAdderService reportAdderService,
        IPromptReportGetterService reportGetterService,
        IPromptReportResolverService reportResolverService)
    {
        _reportAdderService = reportAdderService;
        _reportGetterService = reportGetterService;
        _reportResolverService = reportResolverService;
    }

    // POST: api/reports — any authenticated user can file a report
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(PromptReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PromptReportResponse>> ReportPrompt([FromBody] PromptReportAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return InvalidUserProblem();

        var result = await _reportAdderService.AddReportAsync(userId.Value, request);
        return Ok(result);
    }

    // GET: api/reports/pending — moderation queue
    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedResult<PromptReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        var reports = await _reportGetterService.GetPendingReportsAsync(page, pageSize);
        return Ok(reports);
    }

    // POST: api/reports/{id}/resolve — decide: hide the prompt or dismiss
    [Authorize(Roles = "Admin")]
    [HttpPost("{reportId:guid}/resolve")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve(Guid reportId, [FromBody] ModerationDecisionRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var adminId = GetCurrentUserId();
        if (adminId is null)
            return InvalidUserProblem();

        var result = await _reportResolverService.ResolveAsync(reportId, adminId.Value, request);
        return Ok(result);
    }
}

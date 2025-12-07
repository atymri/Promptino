using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using Promptino.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace Promptino.API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IPromptAdderService _promptAdderService;
    private readonly IPromptUpdaterService _promptUpdaterService;
    private readonly IPromptDeleterService _promptDeleterService;

    private readonly IImageAdderService _imageAdderService;
    private readonly IImageUpdaterService _imageUpdaterService;
    private readonly IImageDeleterService _imageDeleterService;
    private readonly IImageGetterrService _imageGetterService;

    public AdminController(
        IPromptAdderService adderService,
        IPromptUpdaterService updaterService,
        IPromptDeleterService deleterService,
        IImageAdderService imageAdderService,
        IImageUpdaterService imageUpdaterService,
        IImageDeleterService imageDeleterService,
        IImageGetterrService imageGetterService)
    {
        _promptAdderService = adderService;
        _promptUpdaterService = updaterService;
        _promptDeleterService = deleterService;

        _imageAdderService = imageAdderService;
        _imageUpdaterService = imageUpdaterService;
        _imageDeleterService = imageDeleterService;
        _imageGetterService = imageGetterService;
    }

    // ─────────────────────────────── Prompts ───────────────────────────────

    [HttpPost("prompts")]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromptResponse>> CreatePrompt([FromBody] PromptAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _promptAdderService.CreatePromptAsync(request);
        return Ok(result);
    }

    [HttpPut("prompts")]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromptResponse>> UpdatePrompt([FromBody] PromptUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _promptUpdaterService.UpdatePromptAsync(request);
        return Ok(result);
    }

    [HttpDelete("prompts/{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeletePrompt(Guid id)
    {
        var result = await _promptDeleterService.DeletePromptAsync(id);
        return Ok(result);
    }


    // ─────────────────────────────── Images ───────────────────────────────

    [HttpPost("[action]")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImageResponse>> CreateImage([FromForm] ImageFormRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return Problem("فایل تصویر ارسال نشده است",
                statusCode: StatusCodes.Status400BadRequest,
                title: "درخواست نامعتبر");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(request.File.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        var relativePath = "/images/" + uniqueFileName;

        var result = await _imageAdderService.CreateImageAsync(
            new ImageAddRequest(request.Title, relativePath, request.GeneratedWith));

        return Ok(result);
    }


    [HttpPut("[action]")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageResponse>> UpdateImage([FromForm] ImageUpdateFormRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (request.File == null || request.File.Length == 0)
            return Problem(
                detail: "فایل تصویر ارسال نشده است",
                title: "درخواست نامعتبر",
                statusCode: StatusCodes.Status400BadRequest);

        var image = await _imageGetterService.GetImageByConditionAsync(im => im.Id == request.Id);
        if (image == null)
            return Problem("تصویر مورد نظر وجود  ندارد",
                statusCode: StatusCodes.Status404NotFound,
                title: "تصویر پیدا نشد");

        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Path.TrimStart('/'));
        if (System.IO.File.Exists(oldFilePath))
        {
            System.IO.File.Delete(oldFilePath);
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(request.File.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        var relativePath = "/images/" + uniqueFileName;

        var imageUpdateDto = new ImageUpdateRequest(
            Id: request.Id,
            Title: request.Title,
            Path: relativePath,
            file: request.File,
            GeneratedWith: request.GeneratedWith
        );

        var result = await _imageUpdaterService.UpdateImageAsync(imageUpdateDto);

        return Ok(result);
    }


    [HttpDelete("images/{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteImage(Guid id)
    {
        var image = await _imageGetterService.GetImageByConditionAsync(im => im.Id == id);
        if (image == null)
            return Problem("تصویر مورد نظر وجود  ندارد",
                statusCode: StatusCodes.Status404NotFound,
                title: "تصویر پیدا نشد");

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Path.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        var result = await _imageDeleterService.DeleteImageAsync(id);
        return Ok(result);
    }


    [HttpPost("images/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AssignImageToPrompt([FromQuery] Guid promptId, [FromQuery] Guid imageId)
    {
        var result = await _imageAdderService.AddImageToPromptAsync(promptId, imageId);
        return Ok(result);
    }

    [HttpDelete("images/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveImageFromPrompt([FromQuery] Guid promptId, [FromQuery] Guid imageId)
    {
        var result = await _imageDeleterService.RemoveImageFromPromptAsync(promptId, imageId);
        return Ok(result);
    }
}

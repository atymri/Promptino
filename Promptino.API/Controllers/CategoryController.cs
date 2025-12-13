using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts.CategoryServiceContracts;

namespace Promptino.API.Controllers;

public class CategoriesController : BaseController
{
    private readonly ICategoryGetterService _categoryGetterService;
    private readonly ICategoryAdderService _categoryAdderService;
    private readonly ICategoryUpdaterService _categoryUpdaterService;
    private readonly ICategoryDeleterService _categoryDeleterService;

    public CategoriesController(
        ICategoryGetterService categoryGetterService,
        ICategoryAdderService categoryAdderService,
        ICategoryUpdaterService categoryUpdaterService,
        ICategoryDeleterService categoryDeleterService)
    {
        _categoryGetterService = categoryGetterService;
        _categoryAdderService = categoryAdderService;
        _categoryUpdaterService = categoryUpdaterService;
        _categoryDeleterService = categoryDeleterService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryGetterService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{name}")]
    [ProducesResponseType(typeof(List<PromptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryPrompts(string name)
    {
        var prompts = await _categoryGetterService.GetPromptsFromCategoryAsync(name);
        return Ok(prompts);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCategories([FromQuery] string keyword)
    {
        var categories = await _categoryGetterService.SearchCategoriesAsync(keyword);
        return Ok(categories);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(
        [FromBody] CategoryAddRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _categoryAdderService.AddCategoryAsync(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(
        [FromBody] CategoryUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _categoryUpdaterService.UpdateCategory(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{categoryId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> DeleteCategory(Guid categoryId)
    {
        var result = await _categoryDeleterService.DeleteCategory(categoryId);
        return Ok(result);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("assign")]
    public async Task<ActionResult<bool>> AddPromptToCategory(
        [FromBody] AddPromptToCategoryRequest request)
    {
        var result = await _categoryAdderService.AddPromptToCategory(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("assign")]
    public async Task<ActionResult<bool>> RemovePromptFromCategory(
        [FromBody] DeletePromptFromCategoryRequest request)
    {
        var result = await _categoryDeleterService.RemovePromptFromCategorry(request);
        return Ok(result);
    }
}

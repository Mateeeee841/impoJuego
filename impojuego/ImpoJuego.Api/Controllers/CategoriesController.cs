using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImpoJuego.Api.DTOs;
using ImpoJuego.Api.Services;

namespace ImpoJuego.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    private (int? userId, bool isAdmin) GetUserInfo()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        int? userId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var id))
            userId = id;

        return (userId, role == "Admin");
    }

    // GET /api/categories - Lista categorías del usuario (admin ve todas, user solo propias)
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var (userId, isAdmin) = GetUserInfo();
        var categories = await _categoryService.GetCategoriesForUserAsync(userId, isAdmin);

        var result = categories.Select(c => new CategoryDetailDto(
            c.Id,
            c.Name,
            c.Words.Select(w => w.Text).ToList(),
            c.IsSystem,
            c.OwnerId == userId,
            c.IsActive
        )).ToList();

        return Ok(new ApiResponse<List<CategoryDetailDto>>(true, "Categorías obtenidas", result));
    }

    // GET /api/categories/active - Lista categorías activas para jugar (sistema + propias)
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCategories()
    {
        var (userId, _) = GetUserInfo();
        var categories = await _categoryService.GetActiveCategoriesAsync(userId);

        var result = categories.Select(c => new CategoryDetailDto(
            c.Id,
            c.Name,
            c.Words.Select(w => w.Text).ToList(),
            c.IsSystem,
            c.OwnerId == userId,
            c.IsActive
        )).ToList();

        return Ok(new ApiResponse<List<CategoryDetailDto>>(true, "Categorías activas obtenidas", result));
    }

    // GET /api/categories/{id}
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var (userId, isAdmin) = GetUserInfo();
        var category = await _categoryService.GetCategoryByIdAsync(id, userId, isAdmin);

        if (category == null)
            return NotFound(new ApiResponse<object>(false, "Categoría no encontrada", null));

        return Ok(new ApiResponse<CategoryDetailDto>(
            true,
            "Categoría obtenida",
            new CategoryDetailDto(
                category.Id,
                category.Name,
                category.Words.Select(w => w.Text).ToList(),
                category.IsSystem,
                category.OwnerId == userId,
                category.IsActive
            )
        ));
    }

    // POST /api/categories
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var (userId, isAdmin) = GetUserInfo();

        // Solo admin puede crear categorías del sistema
        var isSystem = request.IsSystem && isAdmin;

        var (category, error) = await _categoryService.CreateCategoryAsync(
            request.Name,
            request.Words,
            userId,
            isSystem
        );

        if (error != null)
            return BadRequest(new ApiResponse<object>(false, error, null));

        return Ok(new ApiResponse<CategoryDetailDto>(
            true,
            "Categoría creada exitosamente",
            new CategoryDetailDto(
                category!.Id,
                category.Name,
                category.Words.Select(w => w.Text).ToList(),
                category.IsSystem,
                category.OwnerId == userId,
                category.IsActive
            )
        ));
    }

    // PUT /api/categories/{id}
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var (userId, isAdmin) = GetUserInfo();

        var (category, error) = await _categoryService.UpdateCategoryAsync(
            id,
            request.Name,
            request.Words,
            userId,
            isAdmin
        );

        if (error != null)
            return BadRequest(new ApiResponse<object>(false, error, null));

        return Ok(new ApiResponse<CategoryDetailDto>(
            true,
            "Categoría actualizada exitosamente",
            new CategoryDetailDto(
                category!.Id,
                category.Name,
                category.Words.Select(w => w.Text).ToList(),
                category.IsSystem,
                category.OwnerId == userId,
                category.IsActive
            )
        ));
    }

    // DELETE /api/categories/{id}
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var (userId, isAdmin) = GetUserInfo();

        var (success, error) = await _categoryService.DeleteCategoryAsync(id, userId, isAdmin);

        if (!success)
            return BadRequest(new ApiResponse<object>(false, error ?? "Error al eliminar", null));

        return Ok(new ApiResponse<object>(true, "Categoría eliminada exitosamente", null));
    }

    // POST /api/categories/{id}/toggle - Activar/desactivar categoría
    [Authorize]
    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> ToggleCategory(int id)
    {
        var (userId, isAdmin) = GetUserInfo();

        var (success, error) = await _categoryService.ToggleCategoryActiveAsync(id, userId, isAdmin);

        if (!success)
            return BadRequest(new ApiResponse<object>(false, error ?? "Error al cambiar estado", null));

        return Ok(new ApiResponse<object>(true, "Estado de la categoría actualizado", null));
    }

    // POST /api/categories/import - Importar múltiples categorías desde JSON
    [Authorize]
    [HttpPost("import")]
    public async Task<IActionResult> ImportCategories([FromBody] ImportCategoriesRequest request)
    {
        var (userId, _) = GetUserInfo();

        if (!userId.HasValue)
            return Unauthorized(new ApiResponse<object>(false, "Usuario no autenticado", null));

        if (request.Categories == null || request.Categories.Count == 0)
            return BadRequest(new ApiResponse<object>(false, "No se proporcionaron categorías", null));

        var categoriesData = request.Categories
            .Select(c => (c.Name, c.Words))
            .ToList();

        var (created, failed, errors) = await _categoryService.ImportCategoriesAsync(categoriesData, userId.Value);

        var message = $"{created} categoría(s) creada(s)";
        if (failed > 0)
            message += $", {failed} fallida(s)";

        return Ok(new ApiResponse<ImportResultDto>(
            true,
            message,
            new ImportResultDto(created, failed, errors)
        ));
    }
}

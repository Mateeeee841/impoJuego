using Microsoft.EntityFrameworkCore;
using ImpoJuego.Api.Data;
using impojuego.Data.Entities;

namespace ImpoJuego.Api.Services;

public interface ICategoryService
{
    Task<List<Category>> GetCategoriesForUserAsync(int? userId, bool isAdmin);
    Task<List<Category>> GetActiveCategoriesAsync(int? userId);
    Task<Category?> GetCategoryByIdAsync(int id, int? userId, bool isAdmin);
    Task<(Category? category, string? error)> CreateCategoryAsync(string name, List<string> words, int? ownerId, bool isSystem);
    Task<(Category? category, string? error)> UpdateCategoryAsync(int id, string name, List<string> words, int? userId, bool isAdmin);
    Task<(bool success, string? error)> DeleteCategoryAsync(int id, int? userId, bool isAdmin);
    Task<(bool success, string? error)> ToggleCategoryActiveAsync(int id, int? userId, bool isAdmin);
    Task<(int created, int failed, List<string> errors)> ImportCategoriesAsync(List<(string name, List<string> words)> categories, int userId);
}

public class CategoryService : ICategoryService
{
    private readonly ImpoJuegoDbContext _context;

    public CategoryService(ImpoJuegoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetCategoriesForUserAsync(int? userId, bool isAdmin)
    {
        IQueryable<Category> query = _context.Categories.Include(c => c.Words);

        if (isAdmin)
        {
            // Admin ve todas las categorías
            return await query.OrderBy(c => c.IsSystem ? 0 : 1).ThenBy(c => c.Name).ToListAsync();
        }
        else if (userId.HasValue)
        {
            // Usuario normal solo ve sus propias categorías
            return await query.Where(c => c.OwnerId == userId.Value)
                             .OrderBy(c => c.Name)
                             .ToListAsync();
        }
        else
        {
            // No logueado: no ve nada
            return new List<Category>();
        }
    }

    public async Task<List<Category>> GetActiveCategoriesAsync(int? userId)
    {
        // Solo categorías propias del usuario activas
        if (!userId.HasValue)
        {
            // No logueado: no ve categorías
            return new List<Category>();
        }

        return await _context.Categories
            .Include(c => c.Words)
            .Where(c => c.IsActive && c.OwnerId == userId.Value)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, int? userId, bool isAdmin)
    {
        var category = await _context.Categories
            .Include(c => c.Words)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        // Verificar permisos
        if (isAdmin) return category;
        if (category.OwnerId == userId) return category;

        return null;
    }

    public async Task<(Category? category, string? error)> CreateCategoryAsync(string name, List<string> words, int? ownerId, bool isSystem)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (null, "El nombre de la categoría es requerido");

        if (words == null || words.Count == 0)
            return (null, "La categoría debe tener al menos una palabra");

        var category = new Category
        {
            Name = name.Trim(),
            IsSystem = isSystem,
            IsActive = true,
            OwnerId = isSystem ? null : ownerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        foreach (var word in words.Where(w => !string.IsNullOrWhiteSpace(w)))
        {
            _context.Words.Add(new Word
            {
                Text = word.Trim(),
                CategoryId = category.Id
            });
        }

        await _context.SaveChangesAsync();

        return (category, null);
    }

    public async Task<(Category? category, string? error)> UpdateCategoryAsync(int id, string name, List<string> words, int? userId, bool isAdmin)
    {
        var category = await _context.Categories
            .Include(c => c.Words)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return (null, "Categoría no encontrada");

        // Verificar permisos
        if (!isAdmin && category.OwnerId != userId)
            return (null, "No tenés permiso para editar esta categoría");

        if (string.IsNullOrWhiteSpace(name))
            return (null, "El nombre de la categoría es requerido");

        if (words == null || words.Count == 0)
            return (null, "La categoría debe tener al menos una palabra");

        category.Name = name.Trim();

        // Eliminar palabras existentes
        _context.Words.RemoveRange(category.Words);

        // Agregar nuevas palabras
        foreach (var word in words.Where(w => !string.IsNullOrWhiteSpace(w)))
        {
            _context.Words.Add(new Word
            {
                Text = word.Trim(),
                CategoryId = category.Id
            });
        }

        await _context.SaveChangesAsync();

        // Recargar con las nuevas palabras
        category = await _context.Categories
            .Include(c => c.Words)
            .FirstOrDefaultAsync(c => c.Id == id);

        return (category, null);
    }

    public async Task<(bool success, string? error)> DeleteCategoryAsync(int id, int? userId, bool isAdmin)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return (false, "Categoría no encontrada");

        // Verificar permisos
        if (!isAdmin && category.OwnerId != userId)
            return (false, "No tenés permiso para eliminar esta categoría");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool success, string? error)> ToggleCategoryActiveAsync(int id, int? userId, bool isAdmin)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return (false, "Categoría no encontrada");

        // Verificar permisos
        if (!isAdmin && category.OwnerId != userId)
            return (false, "No tenés permiso para modificar esta categoría");

        category.IsActive = !category.IsActive;
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(int created, int failed, List<string> errors)> ImportCategoriesAsync(List<(string name, List<string> words)> categories, int userId)
    {
        var created = 0;
        var failed = 0;
        var errors = new List<string>();

        foreach (var (name, words) in categories)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                failed++;
                errors.Add("Categoría sin nombre ignorada");
                continue;
            }

            if (words == null || words.Count == 0)
            {
                failed++;
                errors.Add($"'{name}': sin palabras");
                continue;
            }

            // Verificar si ya existe una categoría con ese nombre para este usuario
            var exists = await _context.Categories.AnyAsync(c =>
                c.OwnerId == userId && c.Name.ToLower() == name.Trim().ToLower());

            if (exists)
            {
                failed++;
                errors.Add($"'{name}': ya existe");
                continue;
            }

            var category = new Category
            {
                Name = name.Trim(),
                IsSystem = false,
                IsActive = true,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            foreach (var word in words.Where(w => !string.IsNullOrWhiteSpace(w)))
            {
                _context.Words.Add(new Word
                {
                    Text = word.Trim(),
                    CategoryId = category.Id
                });
            }

            await _context.SaveChangesAsync();
            created++;
        }

        return (created, failed, errors);
    }
}

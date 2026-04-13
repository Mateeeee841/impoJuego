using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using impojuego.Data.Entities;

namespace ImpoJuego.Api.Data;

public static class DbSeeder
{
    public static async Task SeedDatabaseAsync(ImpoJuegoDbContext context)
    {
        // Las migrations ya las corrió Program.cs (context.Database.MigrateAsync).
        // Acá solo sembramos datos de referencia.
        await SeedAdminUserAsync(context);
        await SeedSystemCategoriesAsync(context);
    }

    private static async Task SeedAdminUserAsync(ImpoJuegoDbContext context)
    {
        // Admin se crea solo si hay ADMIN_EMAIL y ADMIN_PASSWORD en env vars.
        // Sin env vars no se crea admin y el seed de categorías tampoco corre.
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            Console.WriteLine("[DbSeeder] ADMIN_EMAIL/ADMIN_PASSWORD no seteados — admin seed saltado.");
            return;
        }

        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        var adminUser = new User
        {
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
        Console.WriteLine($"[DbSeeder] Admin creado: {adminEmail}");
    }

    private static async Task SeedSystemCategoriesAsync(ImpoJuegoDbContext context)
    {
        // Admin viene de env var. Si no hay admin, no se siembran categorías.
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        if (string.IsNullOrWhiteSpace(adminEmail)) return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (admin == null) return;

        // Si el admin ya tiene categorías, no hacer nada
        if (await context.Categories.AnyAsync(c => c.OwnerId == admin.Id))
            return;

        var adminCategories = await LoadDefaultCategoriesAsync();
        if (adminCategories.Count == 0)
        {
            Console.WriteLine("[DbSeeder] No se encontraron categorías default — seed skippeado.");
            return;
        }

        foreach (var (categoryName, words) in adminCategories)
        {
            var category = new Category
            {
                Name = categoryName,
                IsSystem = false,
                IsActive = true,
                OwnerId = admin.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            foreach (var word in words)
            {
                context.Words.Add(new Word
                {
                    Text = word,
                    CategoryId = category.Id
                });
            }

            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Carga las categorías default desde Data/defaultCategories.json.
    /// Si el archivo no existe o está mal formado, retorna diccionario vacío
    /// (seed no corre, la app sigue funcionando).
    /// </summary>
    private static async Task<Dictionary<string, List<string>>> LoadDefaultCategoriesAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "defaultCategories.json");
        if (!File.Exists(path))
        {
            Console.WriteLine($"[DbSeeder] defaultCategories.json no encontrado en {path}");
            return new Dictionary<string, List<string>>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                ?? new Dictionary<string, List<string>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbSeeder] Error leyendo defaultCategories.json: {ex.Message}");
            return new Dictionary<string, List<string>>();
        }
    }
}

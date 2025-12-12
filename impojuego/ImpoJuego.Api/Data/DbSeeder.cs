using Microsoft.EntityFrameworkCore;
using impojuego.Data.Entities;

namespace ImpoJuego.Api.Data;

public static class DbSeeder
{
    public static async Task SeedDatabaseAsync(ImpoJuegoDbContext context)
    {
        // Asegurar que la DB existe
        await context.Database.EnsureCreatedAsync();

        // Seed admin user
        await SeedAdminUserAsync(context);

        // Seed system categories
        await SeedSystemCategoriesAsync(context);
    }

    private static async Task SeedAdminUserAsync(ImpoJuegoDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == "mateocirujas"))
            return;

        var adminUser = new User
        {
            Email = "mateocirujas",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("mateo"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSystemCategoriesAsync(ImpoJuegoDbContext context)
    {
        // Obtener el admin para asignarle las categorías
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "mateocirujas");
        if (admin == null) return;

        // Si el admin ya tiene categorías, no hacer nada
        if (await context.Categories.AnyAsync(c => c.OwnerId == admin.Id))
            return;

        var adminCategories = new Dictionary<string, List<string>>
        {
            ["Conocidos"] = new()
            {
                "Mateo", "Chuvy", "Coza", "Eze", "Guti", "Tadeo", "Ciro", "Julian",
                "Luis", "Marcos C", "Karulo", "Paz", "Ana", "Mili", "Agus Arditi",
                "Barro", "Mama de Chuvy", "Fran Plaza", "Emma Randazo", "Azul",
                "Pauni", "El Negro", "Lisandro Lopez", "Flor Martinez", "Jose",
                "Mama Coza", "Eva", "Barba Gutierrez", "Barba Joan", "Juancito",
                "Alma", "Simon", "Cipri", "Gordo T", "Christian", "Mia",
                "Vicky La Plata", "Sol Juampa", "Emma 24", "Juampa", "Pitusa",
                "Aldy", "Laucha Cuervo", "Nico Cam", "Juan Polo", "Santo", "Sazu",
                "Manu Grande", "Nico Garcia", "Franco Meysner", "Sophi San",
                "Maria Clara", "Wensy", "Pedrotti", "Fiorella", "Facu Durigon",
                "Rama Pennimpede", "Regina", "Barbara Novia Ciro", "Lechon",
                "Sofi Comuzzi", "Male Rafo", "Emilio", "Pasha", "Zaira Nara",
                "Franco Novio Ana", "Agus Arbillaga", "Ale", "Mica Alonso",
                "Pipan", "Lijo"
            },

            ["Famosos"] = new()
            {
                "Coscu", "Spreen", "Momo Benavides", "Frankkaster", "Goncho Banzas",
                "Joaco López", "Carreraaa", "Papo MC", "Pimpeano", "Brunenger",
                "Luquita Rodríguez", "Bauletetti", "Moski", "Speed", "Aguero",
                "Alonso", "Dabo", "La Cobra", "El Wandi", "Oky", "Vegeta777",
                "Pelao Ke", "La Reini", "Kai Cenat", "Guido Kaska", "Brad Pitt",
                "Tom Holland", "Wanda Nara", "Marley", "Tinelli", "Coco Basile",
                "Ginobili", "Barassi", "L-Gante", "Nicolas Detrassy", "Gaston Edul",
                "Fabrizio Romano", "Laurita Fernandez", "El Mago Sin Dientes",
                "Trinche", "Clari Cresmachi", "Martu Crespo"
            },

            ["Futbolistas"] = new()
            {
                "Lionel Messi", "Ángel Di María", "Paulo Dybala", "Lautaro Martínez",
                "Dibu Martínez", "Rodrigo De Paul", "Enzo Fernández", "Julián Álvarez",
                "Nicolás Otamendi", "Marcos Rojo", "Gonzalo Higuaín", "Carlos Tévez",
                "Kun Agüero", "Diego Maradona", "Javier Mascherano", "Cristiano Ronaldo",
                "Kylian Mbappé", "Neymar Jr", "Erling Haaland", "Kevin De Bruyne",
                "Mohamed Salah", "Luka Modric", "Sergio Ramos", "Gerard Piqué",
                "Xavi Hernández", "Andrés Iniesta", "David Beckham", "Zinedine Zidane",
                "Ronaldinho", "Ronaldo Nazário", "Pelé", "Francesco Totti",
                "Wayne Rooney", "Didier Drogba", "Iker Casillas", "Foden",
                "Greenwood", "Kane", "Benedetto"
            },

            ["Cantantes"] = new()
            {
                "Bad Bunny", "J Balvin", "Daddy Yankee", "Anuel AA", "Karol G",
                "Shakira", "Maluma", "Ozuna", "Rosalía", "Becky G", "Duki",
                "Bizarrap", "Nicki Nicole", "María Becerra", "Trueno", "Cazzu",
                "KHEA", "Tiago PZK", "Lit Killah", "L-Gante", "FMK", "Rusherking",
                "YSY A", "Neo Pistea", "Seven Kayne", "Bhavi", "Ecko", "Aleman",
                "Tini Stoessel", "Soledad Pastorutti", "Abel Pintos", "Wos", "Luck Ra"
            },

            ["Presidentes Argentinos"] = new()
            {
                "Raúl Alfonsín", "Carlos Menem", "Néstor Kirchner",
                "Cristina Fernández de Kirchner", "Mauricio Macri",
                "Alberto Fernández", "Javier Milei", "Axel Kicillof"
            },

            ["Películas"] = new()
            {
                "Shrek", "Shrek 2", "Shrek 3", "Up", "Spider-Man Homecoming",
                "El Lobo de Wall Street", "Cars", "Cars 2", "Now You See Me",
                "Fight Club", "Rambo", "Los Increíbles", "Corazón de León",
                "Star Wars", "Baby Driver", "Joker", "Batman El Caballero de la Noche",
                "50 Sombras de Grey", "Transformers", "Pistola Desnuda",
                "Dónde Está el Piloto", "Nueve Reinas", "Relatos Salvajes",
                "Avengers Endgame", "Iron Man", "El Increíble Hulk", "Ford vs Ferrari",
                "Rush", "Heat", "Atrápame Si Puedes", "Titanic", "Rocky", "Avatar",
                "Terminator", "Volver al Futuro", "Juego de Gemelas", "Mulan",
                "Rapunzel", "La Bella y la Bestia", "Frozen", "Cómo Entrenar a tu Dragón",
                "Mi Obra Maestra", "Matilda", "Anora", "La Sustancia", "Jorge el Curioso",
                "1985", "El Robo del Siglo", "La Odisea de los Giles", "21 Blackjack",
                "El Fantasma de Buenos Aires", "Ted", "E.T.", "Wall-E", "Dónde Están las Rubias",
                "El Padrino", "El Padrino 2", "El Señor de los Anillos",
                "El Señor de los Anillos: Las Dos Torres",
                "El Señor de los Anillos: El Retorno del Rey",
                "Interestelar", "Inception", "John Wick", "Gladiador", "El Cisne Negro"
            },

            ["Países"] = new()
            {
                "Argentina", "Brasil", "Chile", "Uruguay", "Paraguay",
                "Bolivia", "Perú", "Colombia", "México", "Estados Unidos",
                "España", "Francia", "Alemania", "Italia", "Japón"
            }
        };

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
}

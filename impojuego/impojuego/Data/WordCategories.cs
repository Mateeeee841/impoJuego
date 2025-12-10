namespace ImpoJuego.Data;

/// <summary>
/// Categorías y palabras del juego
/// </summary>
public static class WordCategories
{
    private static readonly Dictionary<string, List<string>> _categories = new()
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

    /// <summary>
    /// Obtiene todas las categorías disponibles
    /// </summary>
    public static IReadOnlyList<string> GetCategoryNames()
        => _categories.Keys.ToList();

    /// <summary>
    /// Obtiene las palabras de una categoría
    /// </summary>
    public static IReadOnlyList<string> GetWords(string category)
        => _categories.TryGetValue(category, out var words)
            ? words
            : new List<string>();

    /// <summary>
    /// Selecciona una categoría al azar
    /// </summary>
    public static string GetRandomCategory(Random random)
    {
        var keys = _categories.Keys.ToList();
        return keys[random.Next(keys.Count)];
    }

    /// <summary>
    /// Selecciona una palabra al azar de una categoría
    /// </summary>
    public static string GetRandomWord(string category, Random random)
    {
        var words = GetWords(category);
        return words.Count > 0
            ? words[random.Next(words.Count)]
            : string.Empty;
    }

    /// <summary>
    /// Agrega una nueva categoría con palabras
    /// </summary>
    public static void AddCategory(string name, List<string> words)
    {
        _categories[name] = words;
    }

    /// <summary>
    /// Agrega palabras a una categoría existente
    /// </summary>
    public static void AddWordsToCategory(string category, params string[] words)
    {
        if (_categories.TryGetValue(category, out var existing))
            existing.AddRange(words);
    }
}

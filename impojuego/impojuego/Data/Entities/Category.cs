namespace impojuego.Data.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;
    public bool IsActive { get; set; } = true; // Para activar/desactivar en partidas
    public int? OwnerId { get; set; }
    public User? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Word> Words { get; set; } = new List<Word>();
}

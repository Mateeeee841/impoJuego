namespace impojuego.Data.Entities;

public class Word
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

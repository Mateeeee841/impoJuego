namespace ImpoJuego.Models;

/// <summary>
/// Representa un jugador en el juego
/// </summary>
public class Player
{
    public string Name { get; }
    public GameRole Role { get; private set; }
    public bool IsEliminated { get; private set; }
    public int RoundsPlayed { get; private set; }

    public Player(string name)
    {
        Name = name.Trim();
        Role = GameRole.Crewmate;
        IsEliminated = false;
        RoundsPlayed = 0;
    }

    public void AssignRole(GameRole role) => Role = role;

    public void Eliminate() => IsEliminated = true;

    public void IncrementRounds() => RoundsPlayed++;

    /// <summary>
    /// Resetea el estado del jugador para una nueva partida
    /// </summary>
    public void Reset()
    {
        Role = GameRole.Crewmate;
        IsEliminated = false;
        RoundsPlayed = 0;
    }

    /// <summary>
    /// Normaliza el nombre para comparaciones (minúscula, sin espacios extra)
    /// </summary>
    public string NormalizedName => Name.ToLower().Trim();

    public override string ToString() => Name;
}

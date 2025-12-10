namespace ImpoJuego.Config;

/// <summary>
/// Configuración del juego - fácil de modificar
/// </summary>
public class GameSettings
{
    /// <summary>
    /// Mínimo de jugadores para iniciar
    /// </summary>
    public int MinPlayers { get; set; } = 3;

    /// <summary>
    /// Máximo de jugadores permitidos
    /// </summary>
    public int MaxPlayers { get; set; } = 20;

    /// <summary>
    /// Probabilidad de tener 2 impostores (0.0 a 1.0) - solo aplica si hay 5+ jugadores
    /// </summary>
    public double TwoImpostorsProbability { get; set; } = 0.04;

    /// <summary>
    /// Mínimo de jugadores para que pueda haber 2 impostores
    /// </summary>
    public int MinPlayersForTwoImpostors { get; set; } = 5;

    /// <summary>
    /// Si true, los impostores saben quién es su cómplice
    /// </summary>
    public bool ImpostorsKnowEachOther { get; set; } = true;

    /// <summary>
    /// Calcula cuántos impostores habrá en la partida (siempre random)
    /// - Menos de 5 jugadores: siempre 1 impostor
    /// - 5 o más jugadores: 4% de chance de 2 impostores
    /// </summary>
    public int GetImpostorCount(Random random, int playerCount)
    {
        // Solo puede haber 2 impostores si hay 5+ jugadores
        if (playerCount >= MinPlayersForTwoImpostors && random.NextDouble() < TwoImpostorsProbability)
            return 2;

        return 1;
    }
}

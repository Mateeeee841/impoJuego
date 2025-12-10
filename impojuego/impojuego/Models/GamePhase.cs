namespace ImpoJuego.Models;

/// <summary>
/// Fases del juego
/// </summary>
public enum GamePhase
{
    Lobby,       // Registrando jugadores
    RoleReveal,  // Cada jugador ve su rol/palabra
    Discussion,  // Ronda de pistas verbales
    Voting,      // Votación para eliminar
    Finished     // Juego terminado
}

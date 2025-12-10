using ImpoJuego.Config;
using ImpoJuego.Managers;
using ImpoJuego.Models;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var settings = new GameSettings
{
    MinPlayers = 3,
    MaxPlayers = 20,
    TwoImpostorsProbability = 0.03,
    ImpostorsKnowEachOther = true
};

var game = new GameManager(settings);

// === LOBBY: Registro de jugadores ===
Console.WriteLine("╔════════════════════════════════════╗");
Console.WriteLine("║      JUEGO DEL IMPOSTOR            ║");
Console.WriteLine("╚════════════════════════════════════╝\n");

Console.WriteLine("Registra a los jugadores (escribe 'listo' para comenzar):\n");

while (true)
{
    Console.Write("Nuevo jugador: ");
    var input = Console.ReadLine()?.Trim() ?? "";

    if (input.ToLower() == "listo")
    {
        var (canStart, startMsg) = game.StartGame();
        if (!canStart)
        {
            Console.WriteLine($"⚠️ {startMsg}\n");
            continue;
        }
        break;
    }

    if (string.IsNullOrWhiteSpace(input))
        continue;

    var (success, message) = game.RegisterPlayer(input);
    Console.WriteLine(success ? $"✅ {message}" : $"⚠️ {message}");
    Console.WriteLine($"   Jugadores ({game.Players.Count}): {string.Join(", ", game.Players.Players.Select(p => p.Name))}\n");
}

// === INICIO DEL JUEGO ===
Console.Clear();
Console.WriteLine("╔════════════════════════════════════╗");
Console.WriteLine("║        ¡PARTIDA INICIADA!          ║");
Console.WriteLine("╚════════════════════════════════════╝\n");

var impostorCount = game.Players.GetActiveImpostors().Count;
Console.WriteLine($"📢 Categoría: {game.CurrentCategory}");
Console.WriteLine($"👥 Jugadores: {string.Join(", ", game.Players.Players.Select(p => p.Name))}");
Console.WriteLine($"🕵️ Impostores: {impostorCount}\n");

Console.WriteLine("Cada jugador debe escribir su nombre para ver su rol.\n");
Console.WriteLine("Presiona ENTER para continuar...");
Console.ReadLine();

// === FASE DE REVELACIÓN DE ROLES ===
var playersToReveal = new HashSet<string>(
    game.Players.Players.Select(p => p.NormalizedName)
);

while (playersToReveal.Count > 0)
{
    Console.Clear();
    Console.WriteLine($"📢 Categoría: {game.CurrentCategory}");
    Console.WriteLine($"🕵️ Hay {impostorCount} impostor(es) en esta partida\n");
    Console.WriteLine($"Faltan por ver su rol: {playersToReveal.Count} jugadores\n");

    Console.Write("✏️ Escribe tu nombre: ");
    var name = Console.ReadLine()?.Trim().ToLower() ?? "";

    if (!playersToReveal.Contains(name))
    {
        if (game.Players.PlayerExists(name))
            Console.WriteLine("\n⚠️ Ya viste tu rol.");
        else
            Console.WriteLine("\n⚠️ Ese nombre no está en la lista.");

        Console.WriteLine("Presiona ENTER...");
        Console.ReadLine();
        continue;
    }

    var info = game.GetPlayerInfo(name);
    if (info == null)
    {
        Console.WriteLine("\n⚠️ Error al obtener información.");
        Console.ReadLine();
        continue;
    }

    Console.WriteLine();

    if (info.Role == GameRole.Impostor)
    {
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("🤫 TU ROL ES: IMPOSTOR");
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine($"\n📂 Categoría: {info.Category}");
        Console.WriteLine("❌ NO conocés la palabra, tenés que improvisar");

        if (info.FellowImpostors?.Count > 0)
        {
            var others = string.Join(", ", info.FellowImpostors.Select(p => p.Name));
            Console.WriteLine($"\n🤝 Tu(s) cómplice(s): {others}");
        }
    }
    else
    {
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine($"🔑 TU PALABRA ES: {info.SecretWord}");
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine($"\n📂 Categoría: {info.Category}");
        Console.WriteLine("✅ Da pistas sutiles sin revelar la palabra");
    }

    playersToReveal.Remove(name);

    Console.WriteLine("\n─────────────────────────────────────");
    WaitForHandoff();
}

// === FASE DE DISCUSIÓN Y VOTACIÓN ===
game.StartDiscussion();

while (game.CurrentPhase != GamePhase.Finished)
{
    Console.Clear();
    Console.WriteLine($"╔════════════════════════════════════╗");
    Console.WriteLine($"║          RONDA {game.RoundNumber}                   ║");
    Console.WriteLine($"╚════════════════════════════════════╝\n");

    Console.WriteLine($"📢 Categoría: {game.CurrentCategory}");
    Console.WriteLine($"👥 Jugadores activos: {string.Join(", ", game.Players.GetActivePlayers().Select(p => p.Name))}\n");

    Console.WriteLine("📣 FASE DE DISCUSIÓN");
    Console.WriteLine("Cada jugador dice una palabra relacionada (en persona).\n");
    Console.WriteLine("Cuando terminen de hablar, presiona ENTER para votar...");
    Console.ReadLine();

    // Votación
    game.StartVoting();
    Console.Clear();
    Console.WriteLine("╔════════════════════════════════════╗");
    Console.WriteLine("║           VOTACIÓN                 ║");
    Console.WriteLine("╚════════════════════════════════════╝\n");

    var activePlayers = game.Players.GetActivePlayers();
    Console.WriteLine("Jugadores activos:");
    for (int i = 0; i < activePlayers.Count; i++)
    {
        Console.WriteLine($"  {i + 1}. {activePlayers[i].Name}");
    }
    Console.WriteLine($"  0. Skip (no votar)\n");

    foreach (var voter in activePlayers)
    {
        while (true)
        {
            Console.Write($"🗳️ {voter.Name}, ¿por quién votas? (número): ");
            var voteInput = Console.ReadLine()?.Trim() ?? "";

            if (!int.TryParse(voteInput, out int voteIndex))
            {
                Console.WriteLine("   Ingresa un número válido.");
                continue;
            }

            Player? target = null;
            if (voteIndex == 0)
            {
                // Skip
            }
            else if (voteIndex >= 1 && voteIndex <= activePlayers.Count)
            {
                target = activePlayers[voteIndex - 1];
                if (target == voter)
                {
                    Console.WriteLine("   No puedes votar por ti mismo.");
                    continue;
                }
            }
            else
            {
                Console.WriteLine("   Número fuera de rango.");
                continue;
            }

            var (success, msg) = game.Voting.CastVote(voter, target);
            if (success)
            {
                Console.WriteLine($"   ✅ Voto registrado\n");
                break;
            }
            Console.WriteLine($"   ⚠️ {msg}");
        }
    }

    // Procesar resultado
    Console.WriteLine("\n═══════════════════════════════════");
    Console.WriteLine("RESULTADO DE LA VOTACIÓN:");
    Console.WriteLine(game.Voting.GetVoteSummary());
    Console.WriteLine("═══════════════════════════════════\n");

    var (voteResult, gameStatus, message) = game.ProcessVotingResult();
    Console.WriteLine($"📢 {message}\n");

    if (gameStatus != GameResult.InProgress)
    {
        Console.WriteLine("\n" + game.GetWinMessage(gameStatus));
        break;
    }

    Console.WriteLine("Presiona ENTER para la siguiente ronda...");
    Console.ReadLine();
}

// === FIN DEL JUEGO ===
Console.WriteLine("\n╔════════════════════════════════════╗");
Console.WriteLine("║         FIN DEL JUEGO              ║");
Console.WriteLine("╚════════════════════════════════════╝\n");

Console.WriteLine("¿Jugar de nuevo? (s/n): ");
var playAgain = Console.ReadLine()?.Trim().ToLower();
if (playAgain == "s")
{
    game.ResetGame();
    // Reiniciar el programa requeriría un loop externo
    Console.WriteLine("Reinicia el programa para jugar de nuevo.");
}

Console.WriteLine("\n¡Gracias por jugar! 👋");

// === FUNCIONES AUXILIARES ===
void WaitForHandoff()
{
    while (true)
    {
        Console.Write("Escribe 'paso' cuando pases el celular: ");
        var confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
        if (confirm == "paso")
        {
            Console.Clear();
            break;
        }
    }
}

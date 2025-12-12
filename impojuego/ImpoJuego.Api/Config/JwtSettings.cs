namespace ImpoJuego.Api.Config;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ImpoJuego";
    public string Audience { get; set; } = "ImpoJuegoApp";
    public int ExpirationMinutes { get; set; } = 1440; // 24 horas por defecto
}

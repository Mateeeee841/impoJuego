using ImpoJuego.Config;
using ImpoJuego.Managers;

var builder = WebApplication.CreateBuilder(args);

// === SERVICES ===

// Configuración del juego
var gameSettings = new GameSettings
{
    MinPlayers = 3,
    MaxPlayers = 20,
    TwoImpostorsProbability = 0.03,
    ImpostorsKnowEachOther = true
};

// Session Manager - cada navegador tiene su propia partida
builder.Services.AddSingleton(gameSettings);
builder.Services.AddSingleton<GameSessionManager>(sp =>
    new GameSessionManager(gameSettings, TimeSpan.FromHours(4)));

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ImpoJuego API", Version = "v1" });
});

// CORS para Angular (localhost y producción)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5173",  // Vite
                "http://127.0.0.1:4200",
                "https://impojuego-web.onrender.com"  // Producción Render
            )
            .AllowAnyHeader()
            .WithExposedHeaders("X-Session-Id")
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// === MIDDLEWARE ===

// CORS debe ir antes de otros middlewares
app.UseCors("Angular");

// Swagger siempre disponible
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ImpoJuego API v1");
    c.RoutePrefix = string.Empty;  // Swagger en la raíz
});

app.UseAuthorization();
app.MapControllers();

// Info al iniciar
Console.WriteLine("=================================");
Console.WriteLine("  ImpoJuego API");
Console.WriteLine("  http://localhost:5000");
Console.WriteLine("  Swagger: http://localhost:5000");
Console.WriteLine("=================================");

app.Run();

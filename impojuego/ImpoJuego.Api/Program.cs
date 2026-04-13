using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ImpoJuego.Config;
using ImpoJuego.Managers;
using ImpoJuego.Api.Config;
using ImpoJuego.Api.Data;
using ImpoJuego.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Render asigna PORT dinámico via env var
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

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

// Database — path configurable via env var DATABASE_PATH (Render: montar disco persistente)
// Si no hay env var, usa el connection string de appsettings
var dbPath = Environment.GetEnvironmentVariable("DATABASE_PATH");
var connectionString = !string.IsNullOrWhiteSpace(dbPath)
    ? $"Data Source={dbPath}"
    : builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ImpoJuegoDbContext>(options =>
    options.UseSqlite(connectionString));

// JWT Settings — Secret DEBE venir de env var JWTSETTINGS__SECRET en producción
// (doble underscore = separador de sección en ASP.NET config). En Development se
// lee de appsettings.Development.json. Sin secret: la app NO arranca.
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
{
    throw new InvalidOperationException(
        "JWT secret no configurado o muy corto. Setear env var JWTSETTINGS__SECRET " +
        "con al menos 32 caracteres. Generar con: openssl rand -base64 64");
}
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ImpoJuego API", Version = "v1" });

    // Agregar soporte para JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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

// === DATABASE INITIALIZATION ===
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ImpoJuegoDbContext>();
    await DbSeeder.SeedDatabaseAsync(dbContext);
}

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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check para Render y monitoring
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Info al iniciar
Console.WriteLine("=================================");
Console.WriteLine("  ImpoJuego API");
Console.WriteLine($"  Listening on :{port}");
Console.WriteLine("  Swagger en la raíz /");
Console.WriteLine("  Health en /health");
Console.WriteLine("=================================");

app.Run();

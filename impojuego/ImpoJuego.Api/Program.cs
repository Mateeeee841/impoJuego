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

// Database
builder.Services.AddDbContext<ImpoJuegoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? new JwtSettings { Secret = "DefaultSecretKeyForDevelopment12345678!" };
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

// Info al iniciar
Console.WriteLine("=================================");
Console.WriteLine("  ImpoJuego API");
Console.WriteLine("  http://localhost:5000");
Console.WriteLine("  Swagger: http://localhost:5000");
Console.WriteLine("=================================");

app.Run();

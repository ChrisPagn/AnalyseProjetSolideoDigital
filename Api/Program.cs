using Api.Data;
using Api.Data.Entities;
using Microsoft.AspNetCore.Components.WebAssembly.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Base de données : SQLite en dev, MySQL en prod (Pomelo) — sélection via configuration,
// pas de chemin ou de provider hardcodé (Prompt Maître 6.1, 14).
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection manquante dans la configuration.");

builder.Services.AddDbContext<AnalyseProjetDbContext>(options =>
{
    if (databaseProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Mot de passe robuste exigé dès la V1 : l'outil est exposé publiquement (Prompt Maître 7.5).
        options.Password.RequiredLength = 12;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<AnalyseProjetDbContext>()
    .AddSignInManager();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme);

// Rate limiting sur l'authentification — posture de sécurité requise dès la V1 pour un outil
// mono-utilisateur exposé publiquement (Prompt Maître 7.5). Implémentation complète prévue à
// l'étape 2 (Auth minimale) ; le service est enregistré ici pour que l'étape 1 pose le socle.
builder.Services.AddRateLimiter(_ => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AnalyseProjetDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.UseHttpsRedirection();

// Le Client Blazor WebAssembly publié est servi comme contenu statique par ce même conteneur
// (un seul service "app" dans docker-compose.yml, cohérent avec Prompt Maître 7.3).
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

/// <summary>
/// Point d'entrée exposé pour WebApplicationFactory dans les tests d'intégration (section 12).
/// </summary>
public partial class Program;

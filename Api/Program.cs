using System.Threading.RateLimiting;
using Api.Data;
using Api.Data.Entities;
using Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Components.WebAssembly.Server;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

// CORS : uniquement en dev local, où le Client (port 5138) et l'Api (port 5118) tournent sur
// des origines séparées. En production, même conteneur/origine (Prompt Maître 7.3) — pas de CORS.
const string PolitiqueCorsDev = "DevClient";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(PolitiqueCorsDev, policy => policy
            .WithOrigins("http://localhost:5138", "https://localhost:7275")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });
}

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

        // Verrouillage du compte après tentatives échouées répétées — complète le rate limiting
        // HTTP ci-dessous par une protection au niveau du compte lui-même (Prompt Maître 7.5).
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AnalyseProjetDbContext>()
    .AddSignInManager();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Api.Services.IUtilisateurCourantAccessor, Api.Services.HttpUtilisateurCourantAccessor>();
builder.Services.AddScoped<Api.Services.AuthService>();
builder.Services.AddScoped<Api.Services.MaturiteCalculatorService>();
builder.Services.AddScoped<Api.Services.CodeSequenceService>();
builder.Services.AddScoped<Api.Services.TracabiliteService>();
builder.Services.AddScoped<Api.Services.ContradictionDetectorService>();
builder.Services.AddScoped<Api.Services.MarkdownExportService>();
builder.Services.AddScoped<Api.Services.PromptMaitreTransfertService>();
builder.Services.AddScoped<Api.Actions.CreateProjetAction>();
builder.Services.AddScoped<Api.Actions.DeleteClientAction>();
builder.Services.AddScoped<Api.Actions.UpdateStatutPhaseAction>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.HttpOnly = true;

        // Strict en production (même origine, Prompt Maître 7.3) ; Lax en dev local, où Client
        // (5138) et Api (5118) sont sur des origines séparées — Strict bloquerait alors le cookie
        // sur toute requête cross-origin même avec CORS + credentials activés.
        options.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict;

        // SameAsRequest (pas Always) : le cookie est marqué Secure dès que la requête est HTTPS
        // (le cas en production, derrière Caddy — Prompt Maître 7.5), mais reste utilisable en
        // dev local et dans les tests d'intégration qui tournent en HTTP simple (TestServer).
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // Endpoints API : pas de redirection HTML vers une page de login inexistante côté Api,
        // on renvoie un statut HTTP exploitable par le Client Blazor.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// Rate limiting sur l'authentification — posture de sécurité requise dès la V1 pour un outil
// mono-utilisateur exposé publiquement (Prompt Maître 7.5). Politique nommée "login" appliquée
// sur POST /api/auth/login via [EnableRateLimiting("login")].
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AnalyseProjetDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration, app.Logger);
}
else
{
    // En production aussi, le compte admin unique doit exister au démarrage — mais sans seed
    // de données de démonstration (Prompt Maître 9 : outil mono-utilisateur).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AnalyseProjetDbContext>();
    db.Database.Migrate();
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration, app.Logger);
}

// Caddy termine le TLS et transmet en HTTP en interne au conteneur app (Prompt Maître 7.3) :
// sans ceci, l'Api croirait que toutes les requêtes sont en HTTP et ne marquerait jamais le
// cookie Identity comme Secure en production (CookieSecurePolicy.SameAsRequest ci-dessus).
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

// Le Client Blazor WebAssembly publié est servi comme contenu statique par ce même conteneur
// (un seul service "app" dans docker-compose.yml, cohérent avec Prompt Maître 7.3).
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseCors(PolitiqueCorsDev);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

/// <summary>
/// Point d'entrée exposé pour WebApplicationFactory dans les tests d'intégration (section 12).
/// </summary>
public partial class Program;

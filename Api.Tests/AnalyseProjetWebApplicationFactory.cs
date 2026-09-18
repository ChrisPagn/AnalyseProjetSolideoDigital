using Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Tests;

/// <summary>
/// Factory de test : remplace la base SQLite fichier par une base SQLite en mémoire (connexion
/// unique conservée ouverte le temps du test) et fournit des identifiants admin de test via
/// configuration, sans toucher au code de Program.cs (Prompt Maître 12 : WebApplicationFactory).
/// </summary>
public class AnalyseProjetWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmailTest = "admin.test@exemple.fr";
    public const string AdminPasswordTest = "TestAdmin2026!Secure";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminAccount:Email"] = AdminEmailTest,
                ["AdminAccount:Password"] = AdminPasswordTest
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AnalyseProjetDbContext>>();
            services.AddDbContext<AnalyseProjetDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}

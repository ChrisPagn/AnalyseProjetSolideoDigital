using Api.Data;
using Api.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Data;

/// <summary>
/// Tests du socle DbContext posé à l'étape 1 : le schéma se crée sans erreur et la contrainte
/// CHECK sur LienTracabilite (Prompt Maître 4.1 : "au moins un lien requis") est bien appliquée
/// en base, pas seulement en validation applicative.
/// SQLite en mémoire (pas le provider InMemory d'EF Core) car les contraintes CHECK ne sont
/// évaluées que par un vrai provider relationnel.
/// </summary>
public class AnalyseProjetDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;

    public AnalyseProjetDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AnalyseProjetDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Schema_should_be_creatable_from_current_model()
    {
        // La création du schéma dans le constructeur ne doit lever aucune exception ;
        // ce test échoue si elle en lève une avant d'atteindre ce point.
        Assert.True(_db.Database.CanConnect());
    }

    [Fact]
    public async Task LienTracabilite_sans_aucun_lien_doit_etre_rejete()
    {
        var lien = new LienTracabilite();
        _db.LiensTracabilite.Add(lien);

        await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }

    [Fact]
    public async Task LienTracabilite_avec_un_seul_lien_probleme_doit_etre_accepte()
    {
        var client = new Client { Nom = "Client Test" };
        var projet = new Projet { Nom = "Projet Test", Client = client, DateCreation = DateTime.UtcNow };
        var probleme = new Probleme
        {
            Code = "PROB-001",
            Description = "Problème test",
            Gravite = Gravite.Modere,
            Frequence = 1,
            ImpactTempsHeuresMois = 1,
            ScoreCalcule = 1,
            Projet = projet
        };

        _db.Problemes.Add(probleme);
        _db.LiensTracabilite.Add(new LienTracabilite { Probleme = probleme });

        var nbLignes = await _db.SaveChangesAsync();

        Assert.True(nbLignes > 0);
    }

    [Fact]
    public async Task Projet_niveau_maturite_par_defaut_est_zero()
    {
        var client = new Client { Nom = "Client Test" };
        var projet = new Projet { Nom = "Projet Test", Client = client, DateCreation = DateTime.UtcNow };

        _db.Projets.Add(projet);
        await _db.SaveChangesAsync();

        var relu = await _db.Projets.FirstAsync(p => p.Id == projet.Id);
        Assert.Equal(NiveauMaturite.Niveau0Inconnu, relu.NiveauMaturite);
    }
}

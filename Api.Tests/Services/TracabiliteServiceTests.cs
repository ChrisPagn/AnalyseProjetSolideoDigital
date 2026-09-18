using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Services;

public class TracabiliteServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;
    private readonly TracabiliteService _service;

    public TracabiliteServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>().UseSqlite(_connection).Options;
        _db = new AnalyseProjetDbContext(options);
        _db.Database.EnsureCreated();
        _service = new TracabiliteService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<int> CreerProjetAsync()
    {
        var client = new Client { Nom = "Client Test" };
        var projet = new Projet { Nom = "Projet Test", Client = client, DateCreation = DateTime.UtcNow };
        _db.Projets.Add(projet);
        await _db.SaveChangesAsync();
        return projet.Id;
    }

    [Fact]
    public async Task Aucun_orphelin_retourne_liste_vide()
    {
        var projetId = await CreerProjetAsync();

        var alertes = await _service.DetecterOrphelinsAsync(projetId);

        Assert.Empty(alertes);
    }

    [Fact]
    public async Task Fonctionnalite_sans_lien_est_signalee_orpheline()
    {
        var projetId = await CreerProjetAsync();
        _db.Fonctionnalites.Add(new Fonctionnalite
        {
            Code = "F-001",
            ProjetId = projetId,
            Nom = "Fonctionnalité orpheline",
            Priorite = PrioriteMoSCoW.MustHave
        });
        await _db.SaveChangesAsync();

        var alertes = await _service.DetecterOrphelinsAsync(projetId);

        var alerte = Assert.Single(alertes);
        Assert.Equal(TypeAlerteTracabilite.FonctionnaliteOrpheline, alerte.Type);
        Assert.Equal("F-001", alerte.EntiteCode);
    }

    [Fact]
    public async Task Probleme_sans_lien_est_signale_non_couvert()
    {
        var projetId = await CreerProjetAsync();
        _db.Problemes.Add(new Probleme
        {
            Code = "PROB-001",
            ProjetId = projetId,
            Description = "Problème non couvert",
            Gravite = Gravite.Modere,
            Frequence = 1,
            ImpactTempsHeuresMois = 1,
            ScoreCalcule = 1
        });
        await _db.SaveChangesAsync();

        var alertes = await _service.DetecterOrphelinsAsync(projetId);

        var alerte = Assert.Single(alertes);
        Assert.Equal(TypeAlerteTracabilite.BesoinNonCouvert, alerte.Type);
        Assert.Equal("PROB-001", alerte.EntiteCode);
    }

    [Fact]
    public async Task Fonctionnalite_et_probleme_relies_ne_sont_pas_signales()
    {
        var projetId = await CreerProjetAsync();
        var probleme = new Probleme
        {
            Code = "PROB-001",
            ProjetId = projetId,
            Description = "Problème couvert",
            Gravite = Gravite.Modere,
            Frequence = 1,
            ImpactTempsHeuresMois = 1,
            ScoreCalcule = 1
        };
        var fonctionnalite = new Fonctionnalite
        {
            Code = "F-001",
            ProjetId = projetId,
            Nom = "Fonctionnalité couverte",
            Priorite = PrioriteMoSCoW.MustHave
        };
        _db.Problemes.Add(probleme);
        _db.Fonctionnalites.Add(fonctionnalite);
        _db.LiensTracabilite.Add(new LienTracabilite { Probleme = probleme, Fonctionnalite = fonctionnalite });
        await _db.SaveChangesAsync();

        var alertes = await _service.DetecterOrphelinsAsync(projetId);

        Assert.Empty(alertes);
    }
}

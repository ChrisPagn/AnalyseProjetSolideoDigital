using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Services;

/// <summary>
/// Couvre tous les paliers de la formule NiveauMaturite = MIN(NiveauParPhases, NiveauMaxAutorise)
/// listés au Prompt Maître 4.3 (exigence explicite de la section 12 : "tous les cas de blocage").
/// </summary>
public class MaturiteCalculatorServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;
    private readonly MaturiteCalculatorService _service;

    public MaturiteCalculatorServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AnalyseProjetDbContext(options);
        _db.Database.EnsureCreated();
        _service = new MaturiteCalculatorService(_db);
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

    private async Task AjouterPhaseAsync(int projetId, int numero, StatutPhase statut)
    {
        _db.Phases.Add(new Phase
        {
            ProjetId = projetId,
            Numero = numero,
            Nom = $"Phase {numero}",
            Statut = statut,
            DateMaj = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task Aucune_phase_terminee_donne_niveau_0()
    {
        var projetId = await CreerProjetAsync();

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau0Inconnu, niveau);
    }

    [Fact]
    public async Task Phases_01_02_terminees_donne_niveau_1()
    {
        var projetId = await CreerProjetAsync();
        await AjouterPhaseAsync(projetId, 1, StatutPhase.Terminee);
        await AjouterPhaseAsync(projetId, 2, StatutPhase.Terminee);

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau1Comprehension, niveau);
    }

    [Fact]
    public async Task Phases_01_a_04_terminees_donne_niveau_2()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 4; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau2AnalyseMetier, niveau);
    }

    [Fact]
    public async Task Toutes_les_18_phases_terminees_sans_orphelin_donne_niveau_5()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 18; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau5PretPourDev, niveau);
    }

    [Fact]
    public async Task Question_bloquante_ouverte_plafonne_le_niveau_a_1_meme_avec_bloc_a_termine()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 4; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }
        _db.QuestionsRegistre.Add(new QuestionRegistre
        {
            Code = "Q-001",
            ProjetId = projetId,
            Question = "Qui décide en cas de désaccord ?",
            Importance = ImportanceQuestion.Bloquante,
            Statut = StatutQuestion.Ouverte
        });
        await _db.SaveChangesAsync();

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau1Comprehension, niveau);
    }

    [Fact]
    public async Task Question_bloquante_resolue_ne_plafonne_plus_le_niveau()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 4; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }
        _db.QuestionsRegistre.Add(new QuestionRegistre
        {
            Code = "Q-001",
            ProjetId = projetId,
            Question = "Qui décide en cas de désaccord ?",
            Importance = ImportanceQuestion.Bloquante,
            Statut = StatutQuestion.Resolue
        });
        await _db.SaveChangesAsync();

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau2AnalyseMetier, niveau);
    }

    [Fact]
    public async Task Fonctionnalite_orpheline_plafonne_le_niveau_a_4_quand_toutes_phases_terminees()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 18; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }
        _db.Fonctionnalites.Add(new Fonctionnalite
        {
            Code = "F-001",
            ProjetId = projetId,
            Nom = "Fonctionnalité orpheline",
            Priorite = PrioriteMoSCoW.ShouldHave
        });
        await _db.SaveChangesAsync();

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau4Conception, niveau);
    }

    [Fact]
    public async Task Probleme_non_couvert_plafonne_le_niveau_a_4_quand_toutes_phases_terminees()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 18; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }
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

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau4Conception, niveau);
    }

    [Fact]
    public async Task Fonctionnalite_avec_lien_tracabilite_nest_pas_orpheline()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 18; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }
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

        var niveau = await _service.CalculerAsync(projetId);

        Assert.Equal(NiveauMaturite.Niveau5PretPourDev, niveau);
    }

    [Fact]
    public async Task RecalculerEtPersisterAsync_met_a_jour_le_projet_en_base()
    {
        var projetId = await CreerProjetAsync();
        for (var n = 1; n <= 4; n++)
        {
            await AjouterPhaseAsync(projetId, n, StatutPhase.Terminee);
        }

        await _service.RecalculerEtPersisterAsync(projetId);
        await _db.SaveChangesAsync();

        var projet = await _db.Projets.AsNoTracking().FirstAsync(p => p.Id == projetId);
        Assert.Equal(NiveauMaturite.Niveau2AnalyseMetier, projet.NiveauMaturite);
    }
}

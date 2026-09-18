using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Services;

/// <summary>
/// Couvre les 3 cas de contradiction listés au Prompt Maître 4.3 (exigence explicite de la
/// section 12 : "les 3 cas listés en 4.3").
/// </summary>
public class ContradictionDetectorServiceTests : IDisposable
{
    private const string LibelleDemande = "Résumez votre demande en une seule phrase.";

    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;
    private readonly ContradictionDetectorService _service;

    public ContradictionDetectorServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>().UseSqlite(_connection).Options;
        _db = new AnalyseProjetDbContext(options);
        _db.Database.EnsureCreated();
        _service = new ContradictionDetectorService(_db, new CodeSequenceService(_db));
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

    // --- Cas 1 : Demande validée sans Probleme quantifié ---

    [Fact]
    public async Task Demande_validee_sans_probleme_quantifie_genere_une_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = LibelleDemande,
            Valeur = "Je veux une appli mobile",
            Source = SourceInformation.Declaratif,
            Statut = StatutInformation.Valide
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        var contradiction = Assert.Single(contradictions);
        Assert.Equal(TypeContradiction.DemandeSansProblemeQuantifie, contradiction.Type);

        var questions = await _db.QuestionsRegistre.Where(q => q.ProjetId == projetId).ToListAsync();
        var question = Assert.Single(questions);
        Assert.Equal(ImportanceQuestion.Bloquante, question.Importance);
    }

    [Fact]
    public async Task Demande_validee_avec_probleme_quantifie_ne_genere_pas_de_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = LibelleDemande,
            Valeur = "Je veux une appli mobile",
            Source = SourceInformation.Declaratif,
            Statut = StatutInformation.Valide
        });
        _db.Problemes.Add(new Probleme
        {
            Code = "PROB-001",
            ProjetId = projetId,
            Description = "Problème réel quantifié",
            Gravite = Gravite.Important,
            Frequence = 5,
            ImpactTempsHeuresMois = 3,
            ScoreCalcule = 15
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        Assert.DoesNotContain(contradictions, c => c.Type == TypeContradiction.DemandeSansProblemeQuantifie);
    }

    [Fact]
    public async Task Demande_non_validee_ne_genere_pas_de_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = LibelleDemande,
            Valeur = "Je veux une appli mobile",
            Source = SourceInformation.Declaratif,
            Statut = StatutInformation.AConfirmer
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        Assert.DoesNotContain(contradictions, c => c.Type == TypeContradiction.DemandeSansProblemeQuantifie);
    }

    [Fact]
    public async Task Appel_repete_ne_duplique_pas_la_question_de_relance()
    {
        var projetId = await CreerProjetAsync();
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = LibelleDemande,
            Valeur = "Je veux une appli mobile",
            Source = SourceInformation.Declaratif,
            Statut = StatutInformation.Valide
        });
        await _db.SaveChangesAsync();

        await _service.DetecterEtGenererRelancesAsync(projetId);
        await _service.DetecterEtGenererRelancesAsync(projetId);

        var questions = await _db.QuestionsRegistre.Where(q => q.ProjetId == projetId).ToListAsync();
        Assert.Single(questions);
    }

    // --- Cas 2 : InformationRegistre contradictoires ---

    [Fact]
    public async Task Deux_informations_meme_libelle_valeurs_differentes_genere_une_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001", ProjetId = projetId, Libelle = "Budget pressenti", Valeur = "15000€",
            Source = SourceInformation.Declaratif, Statut = StatutInformation.AConfirmer
        });
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-002", ProjetId = projetId, Libelle = "budget pressenti", Valeur = "20000€",
            Source = SourceInformation.Declaratif, Statut = StatutInformation.AConfirmer
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        Assert.Contains(contradictions, c => c.Type == TypeContradiction.InformationsContradictoires);
    }

    [Fact]
    public async Task Deux_informations_meme_libelle_meme_valeur_ne_genere_pas_de_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001", ProjetId = projetId, Libelle = "Budget pressenti", Valeur = "15000€",
            Source = SourceInformation.Declaratif, Statut = StatutInformation.AConfirmer
        });
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-002", ProjetId = projetId, Libelle = "  budget PRESSENTI  ", Valeur = "15000€",
            Source = SourceInformation.Declaratif, Statut = StatutInformation.AConfirmer
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        Assert.DoesNotContain(contradictions, c => c.Type == TypeContradiction.InformationsContradictoires);
    }

    // --- Cas 3 : EtapeProcessus non validée sur Phase 03 Terminee ---

    [Fact]
    public async Task Etape_non_validee_sur_phase_03_terminee_genere_une_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.Phases.Add(new Phase { ProjetId = projetId, Numero = 3, Nom = "Processus métier", Statut = StatutPhase.Terminee, DateMaj = DateTime.UtcNow });
        var processus = new Processus { Code = "PROC-001", ProjetId = projetId, Nom = "Devis" };
        _db.Processus.Add(processus);
        _db.EtapesProcessus.Add(new EtapeProcessus
        {
            Processus = processus, Ordre = 1, Acteur = "Karim", Action = "Visite chantier", ExempleValide = false
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        Assert.Contains(contradictions, c => c.Type == TypeContradiction.EtapeNonValideeSurPhaseTerminee);
    }

    [Fact]
    public async Task Etape_non_validee_sur_phase_03_non_terminee_ne_genere_pas_de_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.Phases.Add(new Phase { ProjetId = projetId, Numero = 3, Nom = "Processus métier", Statut = StatutPhase.EnCours, DateMaj = DateTime.UtcNow });
        var processus = new Processus { Code = "PROC-001", ProjetId = projetId, Nom = "Devis" };
        _db.Processus.Add(processus);
        _db.EtapesProcessus.Add(new EtapeProcessus
        {
            Processus = processus, Ordre = 1, Acteur = "Karim", Action = "Visite chantier", ExempleValide = false
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        Assert.DoesNotContain(contradictions, c => c.Type == TypeContradiction.EtapeNonValideeSurPhaseTerminee);
    }

    [Fact]
    public async Task Toutes_etapes_validees_sur_phase_03_terminee_ne_genere_pas_de_contradiction()
    {
        var projetId = await CreerProjetAsync();
        _db.Phases.Add(new Phase { ProjetId = projetId, Numero = 3, Nom = "Processus métier", Statut = StatutPhase.Terminee, DateMaj = DateTime.UtcNow });
        var processus = new Processus { Code = "PROC-001", ProjetId = projetId, Nom = "Devis" };
        _db.Processus.Add(processus);
        _db.EtapesProcessus.Add(new EtapeProcessus
        {
            Processus = processus, Ordre = 1, Acteur = "Karim", Action = "Visite chantier",
            ExempleValide = true, ExempleDescription = "Devis Dupont vérifié"
        });
        await _db.SaveChangesAsync();

        var contradictions = await _service.DetecterEtGenererRelancesAsync(projetId);

        Assert.DoesNotContain(contradictions, c => c.Type == TypeContradiction.EtapeNonValideeSurPhaseTerminee);
    }
}

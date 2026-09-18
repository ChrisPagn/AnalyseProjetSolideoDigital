using System.IO.Compression;
using System.Text;
using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Services;

public class MarkdownExportServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;
    private readonly MarkdownExportService _service;

    public MarkdownExportServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>().UseSqlite(_connection).Options;
        _db = new AnalyseProjetDbContext(options);
        _db.Database.EnsureCreated();
        _service = new MarkdownExportService(_db, new TracabiliteService(_db));
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<(int ProjetId, List<Phase> Phases)> CreerProjetAvecPhasesAsync()
    {
        var client = new Client { Nom = "Cabinet Verdier & Associés" };
        var projet = new Projet { Nom = "Digitalisation suivi", Client = client, DateCreation = DateTime.UtcNow };
        var phases = new List<Phase>
        {
            new() { Numero = 1, Nom = "Fiche client", Statut = StatutPhase.Terminee, DateMaj = DateTime.UtcNow },
            new() { Numero = 2, Nom = "Découverte du projet", Statut = StatutPhase.EnCours, DateMaj = DateTime.UtcNow },
        };
        foreach (var p in phases)
        {
            projet.Phases.Add(p);
        }

        _db.Projets.Add(projet);
        await _db.SaveChangesAsync();
        return (projet.Id, phases);
    }

    private static Dictionary<string, string> LireEntreesZip(byte[] contenuZip)
    {
        using var stream = new MemoryStream(contenuZip);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var resultat = new Dictionary<string, string>();
        foreach (var entree in archive.Entries)
        {
            using var reader = new StreamReader(entree.Open(), Encoding.UTF8);
            resultat[entree.Name] = reader.ReadToEnd();
        }
        return resultat;
    }

    [Fact]
    public async Task GenererZipAsync_produit_un_fichier_ANALYSE_md_et_un_fichier_par_phase()
    {
        var (projetId, phases) = await CreerProjetAvecPhasesAsync();

        var (nomFichier, contenu) = await _service.GenererZipAsync(projetId);

        Assert.StartsWith("analyse-", nomFichier);
        Assert.EndsWith(".zip", nomFichier);

        var entrees = LireEntreesZip(contenu);
        Assert.Contains("ANALYSE.md", entrees.Keys);
        Assert.Equal(1 + phases.Count, entrees.Count);
    }

    [Fact]
    public async Task Nom_de_fichier_de_phase_suit_le_format_numero_slug()
    {
        var (projetId, _) = await CreerProjetAvecPhasesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);

        var entrees = LireEntreesZip(contenu);
        Assert.Contains("01-fiche-client.md", entrees.Keys);
        Assert.Contains("02-decouverte-du-projet.md", entrees.Keys);
    }

    [Fact]
    public async Task ANALYSE_md_contient_le_nom_du_client_et_le_niveau_de_maturite()
    {
        var (projetId, _) = await CreerProjetAvecPhasesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);

        var entrees = LireEntreesZip(contenu);
        var analyse = entrees["ANALYSE.md"];

        Assert.Contains("Cabinet Verdier & Associés", analyse);
        Assert.Contains("Niveau 0", analyse);
    }

    [Fact]
    public async Task Fichier_de_phase_contient_les_informations_validees_de_cette_phase_uniquement()
    {
        var (projetId, phases) = await CreerProjetAvecPhasesAsync();
        var phase1 = phases[0];
        var phase2 = phases[1];

        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001", ProjetId = projetId, PhaseId = phase1.Id, Libelle = "Nom entreprise",
            Valeur = "Cabinet Verdier", Source = SourceInformation.Declaratif, Statut = StatutInformation.Valide
        });
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-002", ProjetId = projetId, PhaseId = phase2.Id, Libelle = "Activité",
            Valeur = "Comptabilité", Source = SourceInformation.Declaratif, Statut = StatutInformation.Valide
        });
        await _db.SaveChangesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);
        var entrees = LireEntreesZip(contenu);

        Assert.Contains("Nom entreprise", entrees["01-fiche-client.md"]);
        Assert.DoesNotContain("Activité", entrees["01-fiche-client.md"]);
        Assert.Contains("Activité", entrees["02-decouverte-du-projet.md"]);
        Assert.DoesNotContain("Nom entreprise", entrees["02-decouverte-du-projet.md"]);
    }

    [Fact]
    public async Task Fichier_de_phase_signale_les_questions_bloquantes_ouvertes_avec_le_symbole_dedie()
    {
        var (projetId, phases) = await CreerProjetAvecPhasesAsync();
        var phase1 = phases[0];

        _db.QuestionsRegistre.Add(new QuestionRegistre
        {
            Code = "Q-001", ProjetId = projetId, PhaseId = phase1.Id, Question = "Qui décide ?",
            Importance = ImportanceQuestion.Bloquante, Statut = StatutQuestion.Ouverte
        });
        await _db.SaveChangesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);
        var entrees = LireEntreesZip(contenu);

        Assert.Contains("⛔", entrees["01-fiche-client.md"]);
        Assert.Contains("Qui décide ?", entrees["01-fiche-client.md"]);
    }

    [Fact]
    public async Task Question_resolue_napparait_pas_dans_les_points_ouverts()
    {
        var (projetId, phases) = await CreerProjetAvecPhasesAsync();
        var phase1 = phases[0];

        _db.QuestionsRegistre.Add(new QuestionRegistre
        {
            Code = "Q-001", ProjetId = projetId, PhaseId = phase1.Id, Question = "Question résolue",
            Importance = ImportanceQuestion.Normale, Statut = StatutQuestion.Resolue
        });
        await _db.SaveChangesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);
        var entrees = LireEntreesZip(contenu);

        Assert.DoesNotContain("Question résolue", entrees["01-fiche-client.md"]);
    }

    [Fact]
    public async Task Risques_et_decisions_apparaissent_dans_ANALYSE_md_pas_dans_les_fichiers_de_phase()
    {
        var (projetId, _) = await CreerProjetAvecPhasesAsync();

        _db.RisquesRegistre.Add(new RisqueRegistre
        {
            Code = "R-001", ProjetId = projetId, Description = "Dépendance à un seul développeur"
        });
        _db.DecisionsRegistre.Add(new DecisionRegistre
        {
            Code = "DEC-001", ProjetId = projetId, Description = "Utiliser SQLite en dev", Date = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);
        var entrees = LireEntreesZip(contenu);

        Assert.Contains("Dépendance à un seul développeur", entrees["ANALYSE.md"]);
        Assert.Contains("Utiliser SQLite en dev", entrees["ANALYSE.md"]);
        Assert.DoesNotContain("Dépendance à un seul développeur", entrees["01-fiche-client.md"]);
    }

    [Fact]
    public async Task Probleme_non_couvert_apparait_avec_symbole_non_couvert_dans_ANALYSE_md()
    {
        var (projetId, _) = await CreerProjetAvecPhasesAsync();

        _db.Problemes.Add(new Probleme
        {
            Code = "PROB-001", ProjetId = projetId, Description = "Problème non couvert",
            Gravite = Gravite.Important, Frequence = 5, ImpactTempsHeuresMois = 3, ScoreCalcule = 15
        });
        await _db.SaveChangesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);
        var entrees = LireEntreesZip(contenu);

        Assert.Contains("PROB-001", entrees["ANALYSE.md"]);
        Assert.Contains("⛔", entrees["ANALYSE.md"]);
    }

    [Fact]
    public async Task Alertes_de_tracabilite_apparaissent_dans_ANALYSE_md()
    {
        var (projetId, _) = await CreerProjetAvecPhasesAsync();

        _db.Fonctionnalites.Add(new Fonctionnalite
        {
            Code = "F-001", ProjetId = projetId, Nom = "Fonctionnalité orpheline", Priorite = PrioriteMoSCoW.MustHave
        });
        await _db.SaveChangesAsync();

        var (_, contenu) = await _service.GenererZipAsync(projetId);
        var entrees = LireEntreesZip(contenu);

        Assert.Contains("orpheline", entrees["ANALYSE.md"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Projet_inexistant_leve_une_exception()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenererZipAsync(999999));
    }
}

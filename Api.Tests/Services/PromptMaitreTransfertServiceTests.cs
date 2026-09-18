using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Services;

public class PromptMaitreTransfertServiceTests : IDisposable
{
    private const string LibelleDemande = "Résumez votre demande en une seule phrase.";

    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;
    private readonly PromptMaitreTransfertService _service;

    public PromptMaitreTransfertServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>().UseSqlite(_connection).Options;
        _db = new AnalyseProjetDbContext(options);
        _db.Database.EnsureCreated();
        _service = new PromptMaitreTransfertService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<int> CreerProjetAsync(string? stack = "Blazor")
    {
        var client = new Client { Nom = "Association Vivre Ensemble" };
        var projet = new Projet { Nom = "Gestion adhésions", Client = client, DateCreation = DateTime.UtcNow, StackEnvisagee = stack };
        _db.Projets.Add(projet);
        await _db.SaveChangesAsync();
        return projet.Id;
    }

    [Fact]
    public async Task Genere_les_5_sections_attendues()
    {
        var projetId = await CreerProjetAsync();

        var contenu = await _service.GenererAsync(projetId);

        Assert.Contains("## 1. CONTEXTE DU PROJET", contenu);
        Assert.Contains("## 2. STACK TECHNIQUE", contenu);
        Assert.Contains("## 4. DOMAINE MÉTIER", contenu);
        Assert.Contains("## 9. RÔLES & PERMISSIONS", contenu);
        Assert.Contains("## 10. MODULES", contenu);
    }

    [Fact]
    public async Task Section1_reprend_la_demande_validee_en_phase_02()
    {
        var projetId = await CreerProjetAsync();
        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001", ProjetId = projetId, Libelle = LibelleDemande,
            Valeur = "Je veux automatiser les relances de cotisations", Source = SourceInformation.Declaratif,
            Statut = StatutInformation.Valide
        });
        await _db.SaveChangesAsync();

        var contenu = await _service.GenererAsync(projetId);

        Assert.Contains("Je veux automatiser les relances de cotisations", contenu);
    }

    [Fact]
    public async Task Section1_liste_au_plus_3_problemes_les_mieux_priorises()
    {
        var projetId = await CreerProjetAsync();
        for (var i = 1; i <= 5; i++)
        {
            _db.Problemes.Add(new Probleme
            {
                Code = $"PROB-00{i}", ProjetId = projetId, Description = $"Problème {i}",
                Gravite = Gravite.Modere, Frequence = i, ImpactTempsHeuresMois = i, ScoreCalcule = i * i
            });
        }
        await _db.SaveChangesAsync();

        var contenu = await _service.GenererAsync(projetId);

        // Les scores sont 1,4,9,16,25 -> les 3 meilleurs sont Problème 5, 4, 3
        Assert.Contains("Problème 5", contenu);
        Assert.Contains("Problème 4", contenu);
        Assert.Contains("Problème 3", contenu);
        Assert.DoesNotContain("Problème 1", contenu);
    }

    [Fact]
    public async Task Section2_reprend_la_stack_envisagee_du_projet()
    {
        var projetId = await CreerProjetAsync(stack: "Laravel");

        var contenu = await _service.GenererAsync(projetId);

        Assert.Contains("Laravel", contenu);
    }

    [Fact]
    public async Task Section4_liste_les_entites_avec_leurs_attributs()
    {
        var projetId = await CreerProjetAsync();
        _db.Entites.Add(new Entite
        {
            Code = "ENT-001", ProjetId = projetId, Nom = "Adhérent", Attributs = "Nom, email, cotisation"
        });
        await _db.SaveChangesAsync();

        var contenu = await _service.GenererAsync(projetId);

        Assert.Contains("Adhérent", contenu);
        Assert.Contains("Nom, email, cotisation", contenu);
    }

    [Fact]
    public async Task Section4_liste_les_fonctionnalites_triees_par_priorite_moscow()
    {
        var projetId = await CreerProjetAsync();
        _db.Fonctionnalites.Add(new Fonctionnalite
        {
            Code = "F-002", ProjetId = projetId, Nom = "Fonctionnalité secondaire", Priorite = PrioriteMoSCoW.CouldHave
        });
        _db.Fonctionnalites.Add(new Fonctionnalite
        {
            Code = "F-001", ProjetId = projetId, Nom = "Fonctionnalité principale", Priorite = PrioriteMoSCoW.MustHave
        });
        await _db.SaveChangesAsync();

        var contenu = await _service.GenererAsync(projetId);

        var indexF001 = contenu.IndexOf("F-001", StringComparison.Ordinal);
        var indexF002 = contenu.IndexOf("F-002", StringComparison.Ordinal);
        Assert.True(indexF001 < indexF002, "F-001 (MustHave) doit apparaître avant F-002 (CouldHave)");
    }

    [Fact]
    public async Task Section9_resume_les_permissions_dun_acteur()
    {
        var projetId = await CreerProjetAsync();
        var acteur = new Acteur { Code = "ACT-001", ProjetId = projetId, Nom = "Trésorière", Fonction = "Suivi cotisations" };
        _db.Acteurs.Add(acteur);
        await _db.SaveChangesAsync();

        _db.Permissions.Add(new Permission
        {
            ActeurId = acteur.Id, EntiteConcernee = "QuestionRegistre", PeutVoir = true, PeutCreer = true,
            PeutModifier = false, PeutSupprimer = false, PeutValider = false
        });
        await _db.SaveChangesAsync();

        var contenu = await _service.GenererAsync(projetId);

        Assert.Contains("Trésorière", contenu);
        Assert.Contains("QuestionRegistre", contenu);
        Assert.Contains("Voir,Créer", contenu);
    }

    [Fact]
    public async Task Section9_resume_une_permission_totale_par_Tout()
    {
        var projetId = await CreerProjetAsync();
        var acteur = new Acteur { Code = "ACT-001", ProjetId = projetId, Nom = "Présidente" };
        _db.Acteurs.Add(acteur);
        await _db.SaveChangesAsync();

        _db.Permissions.Add(new Permission
        {
            ActeurId = acteur.Id, EntiteConcernee = "Tout le domaine", PeutVoir = true, PeutCreer = true,
            PeutModifier = true, PeutSupprimer = true, PeutValider = true
        });
        await _db.SaveChangesAsync();

        var contenu = await _service.GenererAsync(projetId);

        Assert.Contains("Tout", contenu);
    }

    [Fact]
    public async Task Section10_precise_explicitement_quil_sagit_dune_proposition()
    {
        var projetId = await CreerProjetAsync();

        var contenu = await _service.GenererAsync(projetId);

        Assert.Contains("proposition", contenu, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Projet_inexistant_leve_une_exception()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenererAsync(999999));
    }
}

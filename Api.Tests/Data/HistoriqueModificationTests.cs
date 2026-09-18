using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Data;

/// <summary>
/// Vérifie le mécanisme générique de journalisation (AnalyseProjetDbContext.SaveChangesAsync
/// override, Prompt Maître 5.5) : toute modification de champ scalaire sur une entité de
/// registre doit produire une ligne HistoriqueModification, sans code spécifique à écrire.
/// </summary>
public class HistoriqueModificationTests : IDisposable
{
    private class UtilisateurCourantDeTest : IUtilisateurCourantAccessor
    {
        public string ObtenirIdentifiant() => "test@exemple.fr";
    }

    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;

    public HistoriqueModificationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>().UseSqlite(_connection).Options;
        _db = new AnalyseProjetDbContext(options, new UtilisateurCourantDeTest());
        _db.Database.EnsureCreated();
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
    public async Task Creation_dune_information_ne_genere_aucune_entree_historique()
    {
        var projetId = await CreerProjetAsync();

        _db.InformationsRegistre.Add(new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = "Nom client",
            Source = SourceInformation.Declaratif
        });
        await _db.SaveChangesAsync();

        var entrees = await _db.HistoriqueModifications.ToListAsync();
        Assert.Empty(entrees);
    }

    [Fact]
    public async Task Modification_dun_champ_genere_une_entree_historique()
    {
        var projetId = await CreerProjetAsync();
        var information = new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = "Nom client",
            Valeur = "Ancienne valeur",
            Source = SourceInformation.Declaratif,
            Statut = StatutInformation.AConfirmer
        };
        _db.InformationsRegistre.Add(information);
        await _db.SaveChangesAsync();

        information.Valeur = "Nouvelle valeur";
        await _db.SaveChangesAsync();

        var entrees = await _db.HistoriqueModifications
            .Where(h => h.EntiteType == nameof(InformationRegistre) && h.EntiteId == information.Id)
            .ToListAsync();

        Assert.Single(entrees);
        var entree = entrees[0];
        Assert.Equal("Valeur", entree.Champ);
        Assert.Equal("Ancienne valeur", entree.AncienneValeur);
        Assert.Equal("Nouvelle valeur", entree.NouvelleValeur);
        Assert.Equal("test@exemple.fr", entree.ModifiePar);
    }

    [Fact]
    public async Task Modification_de_plusieurs_champs_en_une_fois_genere_plusieurs_entrees()
    {
        var projetId = await CreerProjetAsync();
        var information = new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = "Ancien libellé",
            Statut = StatutInformation.Inconnu,
            Source = SourceInformation.Declaratif
        };
        _db.InformationsRegistre.Add(information);
        await _db.SaveChangesAsync();

        information.Libelle = "Nouveau libellé";
        information.Statut = StatutInformation.Valide;
        await _db.SaveChangesAsync();

        var entrees = await _db.HistoriqueModifications
            .Where(h => h.EntiteType == nameof(InformationRegistre) && h.EntiteId == information.Id)
            .ToListAsync();

        Assert.Equal(2, entrees.Count);
        Assert.Contains(entrees, e => e.Champ == "Libelle");
        Assert.Contains(entrees, e => e.Champ == "Statut");
    }

    [Fact]
    public async Task Modification_sans_changement_reel_de_valeur_ne_genere_pas_dentree()
    {
        var projetId = await CreerProjetAsync();
        var information = new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = "Libellé stable",
            Source = SourceInformation.Declaratif
        };
        _db.InformationsRegistre.Add(information);
        await _db.SaveChangesAsync();

        // Réassigner exactement la même valeur : EF Core marque la propriété IsModified=true
        // (elle a été touchée) mais aucune vraie divergence de valeur ne doit être journalisée.
        information.Libelle = "Libellé stable";
        _db.Entry(information).Property(i => i.Libelle).IsModified = true;
        await _db.SaveChangesAsync();

        var entrees = await _db.HistoriqueModifications.ToListAsync();
        Assert.Empty(entrees);
    }

    [Fact]
    public async Task Suppression_dune_information_ne_genere_pas_dentree_historique()
    {
        var projetId = await CreerProjetAsync();
        var information = new InformationRegistre
        {
            Code = "INF-001",
            ProjetId = projetId,
            Libelle = "À supprimer",
            Source = SourceInformation.Declaratif
        };
        _db.InformationsRegistre.Add(information);
        await _db.SaveChangesAsync();

        _db.InformationsRegistre.Remove(information);
        await _db.SaveChangesAsync();

        var entrees = await _db.HistoriqueModifications.ToListAsync();
        Assert.Empty(entrees);
    }

    [Fact]
    public async Task Modification_dune_entite_hors_registres_nest_pas_journalisee()
    {
        var projetId = await CreerProjetAsync();
        var projet = await _db.Projets.FirstAsync(p => p.Id == projetId);

        projet.Nom = "Nom modifié";
        await _db.SaveChangesAsync();

        var entrees = await _db.HistoriqueModifications.ToListAsync();
        Assert.Empty(entrees);
    }

    [Fact]
    public async Task Modification_dune_question_registre_est_journalisee()
    {
        var projetId = await CreerProjetAsync();
        var question = new QuestionRegistre
        {
            Code = "Q-001",
            ProjetId = projetId,
            Question = "Qui décide ?",
            Importance = ImportanceQuestion.Normale,
            Statut = StatutQuestion.Ouverte
        };
        _db.QuestionsRegistre.Add(question);
        await _db.SaveChangesAsync();

        question.Statut = StatutQuestion.Resolue;
        await _db.SaveChangesAsync();

        var entrees = await _db.HistoriqueModifications
            .Where(h => h.EntiteType == nameof(QuestionRegistre) && h.EntiteId == question.Id)
            .ToListAsync();

        Assert.Single(entrees);
        Assert.Equal("Statut", entrees[0].Champ);
    }
}

using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Domaine;
using Shared.Dtos.Projets;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class DomaineControllerTests : IAsyncLifetime
{
    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _projetId;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();

        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client pour domaine", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        var creationProjet = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Projet pour domaine", client.Id, null, StatutProjet.EnCours));
        var projet = (await creationProjet.Content.ReadFromJsonAsync<ProjetDto>())!;
        _projetId = projet.Id;

        // Toutes les 18 phases terminées pour atteindre le plafond max autorisé par NiveauParPhases
        var phases = await _client.GetFromJsonAsync<List<Shared.Dtos.Phases.PhaseDto>>($"api/projets/{_projetId}/phases");
        foreach (var phase in phases!)
        {
            await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase.Id}/statut",
                new Shared.Dtos.Phases.UpdateStatutPhaseDto(StatutPhase.Terminee));
        }
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    // --- Probleme ---

    [Fact]
    public async Task Creer_probleme_calcule_le_score_cote_serveur()
    {
        var dto = new UpsertProblemeDto("Relances manuelles oubliées", Gravite.Important, 12, 6, null);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/problemes", dto);
        var probleme = await reponse.Content.ReadFromJsonAsync<ProblemeDto>();

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.Equal("PROB-001", probleme!.Code);
        Assert.Equal(72, probleme.ScoreCalcule);
        Assert.False(probleme.EstCouvert);
    }

    [Fact]
    public async Task GetTous_problemes_les_retourne_tries_par_score_decroissant()
    {
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/problemes",
            new UpsertProblemeDto("Score faible", Gravite.Faible, 1, 1, null));
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/problemes",
            new UpsertProblemeDto("Score élevé", Gravite.Critique, 10, 10, null));

        var reponse = await _client.GetAsync($"api/projets/{_projetId}/problemes");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);

        var problemes = await reponse.Content.ReadFromJsonAsync<List<ProblemeDto>>();
        Assert.NotNull(problemes);
        Assert.Equal(2, problemes.Count);
        Assert.Equal("Score élevé", problemes[0].Description);
        Assert.Equal("Score faible", problemes[1].Description);
    }

    [Fact]
    public async Task Probleme_non_couvert_plafonne_la_maturite_a_4()
    {
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/problemes",
            new UpsertProblemeDto("Non couvert", Gravite.Modere, 1, 1, null));

        var projet = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");

        Assert.Equal(NiveauMaturite.Niveau4Conception, projet!.NiveauMaturite);
    }

    // --- Processus / EtapeProcessus ---

    [Fact]
    public async Task Creer_processus_puis_ajouter_etape_avec_exemple_valide()
    {
        var creationProcessus = await _client.PostAsJsonAsync($"api/projets/{_projetId}/processus",
            new UpsertProcessusDto("Établissement d'un devis", "Appel client"));
        var processus = await creationProcessus.Content.ReadFromJsonAsync<ProcessusDto>();
        Assert.Equal("PROC-001", processus!.Code);

        var etapeDto = new UpsertEtapeProcessusDto(1, "Karim Martin", "Visite du chantier", "Carnet papier", "45 min", null, true, "Devis Dupont vérifié");
        var reponseEtape = await _client.PostAsJsonAsync($"api/projets/{_projetId}/processus/{processus.Id}/etapes", etapeDto);
        var etape = await reponseEtape.Content.ReadFromJsonAsync<EtapeProcessusDto>();

        Assert.True(etape!.ExempleValide);

        var processusRelu = await _client.GetFromJsonAsync<ProcessusDto>($"api/projets/{_projetId}/processus/{processus.Id}");
        Assert.Single(processusRelu!.Etapes);
    }

    // --- Acteur / Permission ---

    [Fact]
    public async Task Creer_acteur_puis_ajouter_permission()
    {
        var creationActeur = await _client.PostAsJsonAsync($"api/projets/{_projetId}/acteurs",
            new UpsertActeurDto("Trésorière bénévole", "Suivi des cotisations"));
        var acteur = await creationActeur.Content.ReadFromJsonAsync<ActeurDto>();
        Assert.Equal("ACT-001", acteur!.Code);

        var permissionDto = new UpsertPermissionDto("QuestionRegistre", true, true, false, false, false);
        var reponsePermission = await _client.PostAsJsonAsync($"api/projets/{_projetId}/acteurs/{acteur.Id}/permissions", permissionDto);
        var permission = await reponsePermission.Content.ReadFromJsonAsync<PermissionDto>();

        Assert.True(permission!.PeutVoir);
        Assert.False(permission.PeutSupprimer);

        var acteurRelu = await _client.GetFromJsonAsync<ActeurDto>($"api/projets/{_projetId}/acteurs/{acteur.Id}");
        Assert.Single(acteurRelu!.Permissions);
    }

    // --- Entite ---

    [Fact]
    public async Task Creer_entite_genere_un_code_ENT_001()
    {
        var dto = new UpsertEntiteDto("Adhérent", "Personne membre de l'association", "Nom, email, cotisation", null);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/entites", dto);
        var entite = await reponse.Content.ReadFromJsonAsync<EntiteDto>();

        Assert.Equal("ENT-001", entite!.Code);
    }

    // --- DocumentMetier ---

    [Fact]
    public async Task Creer_document_metier_genere_un_code_DOC_001()
    {
        var dto = new UpsertDocumentMetierDto("Facture", "Comptabilité", "Client", "PDF", "10 ans");

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/documents", dto);
        var document = await reponse.Content.ReadFromJsonAsync<DocumentMetierDto>();

        Assert.Equal("DOC-001", document!.Code);
    }

    // --- Automatisation ---

    [Fact]
    public async Task Creer_automatisation_genere_un_code_AUTO_001()
    {
        var dto = new UpsertAutomatisationDto("Cotisation impayée 30 jours", null, "Envoyer email de relance", true);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/automatisations", dto);
        var automatisation = await reponse.Content.ReadFromJsonAsync<AutomatisationDto>();

        Assert.Equal("AUTO-001", automatisation!.Code);
    }

    // --- Fonctionnalite / CritereAcceptation / LienTracabilite ---

    [Fact]
    public async Task Creer_fonctionnalite_est_orpheline_par_defaut()
    {
        var dto = new UpsertFonctionnaliteDto(null, "Relance automatique", null, PrioriteMoSCoW.MustHave, StatutFonctionnalite.Identifiee);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites", dto);
        var fonctionnalite = await reponse.Content.ReadFromJsonAsync<FonctionnaliteDto>();

        Assert.Equal("F-001", fonctionnalite!.Code);
        Assert.True(fonctionnalite.EstOrpheline);
    }

    [Fact]
    public async Task Fonctionnalite_orpheline_plafonne_la_maturite_a_4()
    {
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites",
            new UpsertFonctionnaliteDto(null, "Fonctionnalité orpheline", null, PrioriteMoSCoW.ShouldHave, StatutFonctionnalite.Identifiee));

        var projet = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");

        Assert.Equal(NiveauMaturite.Niveau4Conception, projet!.NiveauMaturite);
    }

    [Fact]
    public async Task Creer_fonctionnalite_avec_acteur_inexistant_retourne_bad_request()
    {
        var dto = new UpsertFonctionnaliteDto(999999, "Test", null, PrioriteMoSCoW.CouldHave, StatutFonctionnalite.Identifiee);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites", dto);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Ajouter_critere_acceptation_a_une_fonctionnalite()
    {
        var creationFonctionnalite = await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites",
            new UpsertFonctionnaliteDto(null, "Export PDF", null, PrioriteMoSCoW.ShouldHave, StatutFonctionnalite.Identifiee));
        var fonctionnalite = await creationFonctionnalite.Content.ReadFromJsonAsync<FonctionnaliteDto>();

        var critereDto = new UpsertCritereAcceptationDto("un projet existe", "l'utilisateur exporte en PDF", "le fichier est téléchargé");
        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites/{fonctionnalite!.Id}/criteres", critereDto);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);

        var fonctionnaliteRelue = await _client.GetFromJsonAsync<FonctionnaliteDto>($"api/projets/{_projetId}/fonctionnalites/{fonctionnalite.Id}");
        Assert.Single(fonctionnaliteRelue!.CriteresAcceptation);
    }

    [Fact]
    public async Task Lien_tracabilite_couvre_probleme_et_fonctionnalite_et_debloque_la_maturite()
    {
        var creationProbleme = await _client.PostAsJsonAsync($"api/projets/{_projetId}/problemes",
            new UpsertProblemeDto("Problème à couvrir", Gravite.Important, 5, 3, null));
        var probleme = await creationProbleme.Content.ReadFromJsonAsync<ProblemeDto>();

        var creationFonctionnalite = await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites",
            new UpsertFonctionnaliteDto(null, "Fonctionnalité couvrante", null, PrioriteMoSCoW.MustHave, StatutFonctionnalite.Identifiee));
        var fonctionnalite = await creationFonctionnalite.Content.ReadFromJsonAsync<FonctionnaliteDto>();

        var projetAvantLien = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");
        Assert.Equal(NiveauMaturite.Niveau4Conception, projetAvantLien!.NiveauMaturite);

        var lienDto = new CreerLienTracabiliteDto(probleme!.Id, fonctionnalite!.Id, null, null);
        var reponseLien = await _client.PostAsJsonAsync($"api/projets/{_projetId}/liens-tracabilite", lienDto);
        Assert.Equal(HttpStatusCode.Created, reponseLien.StatusCode);

        var projetApresLien = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");
        Assert.Equal(NiveauMaturite.Niveau5PretPourDev, projetApresLien!.NiveauMaturite);

        var problemeRelu = await _client.GetFromJsonAsync<ProblemeDto>($"api/projets/{_projetId}/problemes/{probleme.Id}");
        Assert.True(problemeRelu!.EstCouvert);
    }

    [Fact]
    public async Task Lien_tracabilite_sans_aucun_lien_retourne_bad_request()
    {
        var dto = new CreerLienTracabiliteDto(null, null, null, null);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/liens-tracabilite", dto);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Supprimer_lien_tracabilite_rend_le_probleme_de_nouveau_non_couvert()
    {
        var creationProbleme = await _client.PostAsJsonAsync($"api/projets/{_projetId}/problemes",
            new UpsertProblemeDto("Problème", Gravite.Faible, 1, 1, null));
        var probleme = await creationProbleme.Content.ReadFromJsonAsync<ProblemeDto>();

        var creationFonctionnalite = await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites",
            new UpsertFonctionnaliteDto(null, "Fonctionnalité", null, PrioriteMoSCoW.CouldHave, StatutFonctionnalite.Identifiee));
        var fonctionnalite = await creationFonctionnalite.Content.ReadFromJsonAsync<FonctionnaliteDto>();

        var reponseLien = await _client.PostAsJsonAsync($"api/projets/{_projetId}/liens-tracabilite",
            new CreerLienTracabiliteDto(probleme!.Id, fonctionnalite!.Id, null, null));
        var lien = await reponseLien.Content.ReadFromJsonAsync<LienTracabiliteDto>();

        await _client.DeleteAsync($"api/projets/{_projetId}/liens-tracabilite/{lien!.Id}");

        var problemeRelu = await _client.GetFromJsonAsync<ProblemeDto>($"api/projets/{_projetId}/problemes/{probleme.Id}");
        Assert.False(problemeRelu!.EstCouvert);
    }
}

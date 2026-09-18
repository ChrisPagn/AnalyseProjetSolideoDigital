using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Domaine;
using Shared.Dtos.Projets;
using Shared.Dtos.Registres;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class TracabiliteControllerTests : IAsyncLifetime
{
    private const string LibelleDemande = "Résumez votre demande en une seule phrase.";

    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _projetId;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();

        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client pour tracabilite", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        var creationProjet = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Projet pour tracabilite", client.Id, null, StatutProjet.EnCours));
        var projet = (await creationProjet.Content.ReadFromJsonAsync<ProjetDto>())!;
        _projetId = projet.Id;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAlertes_sans_orphelin_retourne_liste_vide()
    {
        var alertes = await _client.GetFromJsonAsync<List<AlerteTracabiliteDto>>($"api/projets/{_projetId}/tracabilite/alertes");

        Assert.NotNull(alertes);
        Assert.Empty(alertes);
    }

    [Fact]
    public async Task GetAlertes_signale_une_fonctionnalite_orpheline()
    {
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/fonctionnalites",
            new UpsertFonctionnaliteDto(null, "Fonctionnalité orpheline", null, PrioriteMoSCoW.ShouldHave, StatutFonctionnalite.Identifiee));

        var alertes = await _client.GetFromJsonAsync<List<AlerteTracabiliteDto>>($"api/projets/{_projetId}/tracabilite/alertes");

        Assert.NotNull(alertes);
        Assert.Contains(alertes, a => a.Type == TypeAlerteTracabilite.FonctionnaliteOrpheline);
    }

    [Fact]
    public async Task DetecterContradictions_sur_projet_inexistant_retourne_not_found()
    {
        var reponse = await _client.PostAsync("api/projets/999999/tracabilite/detecter-contradictions", null);

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task DetecterContradictions_demande_sans_probleme_genere_question_bloquante_et_plafonne_maturite()
    {
        // Phases 01-04 terminées pour atteindre Niveau 2 par défaut, afin d'observer réellement
        // l'effet du plafonnement à 1 par la question bloquante générée (sinon NiveauParPhases
        // resterait à 0 et masquerait le plafonnement, comme pour le bug corrigé à l'étape 4/5/6).
        var phases = await _client.GetFromJsonAsync<List<Shared.Dtos.Phases.PhaseDto>>($"api/projets/{_projetId}/phases");
        Assert.NotNull(phases);
        foreach (var phase in phases!.Where(p => p.Numero <= 4))
        {
            await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase.Id}/statut",
                new Shared.Dtos.Phases.UpdateStatutPhaseDto(StatutPhase.Terminee));
        }

        await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations",
            new UpsertInformationRegistreDto(null, LibelleDemande, "Je veux une appli mobile", SourceInformation.Declaratif, StatutInformation.Valide));

        var reponse = await _client.PostAsync($"api/projets/{_projetId}/tracabilite/detecter-contradictions", null);
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);

        var contradictions = await reponse.Content.ReadFromJsonAsync<List<ContradictionDto>>();
        Assert.NotNull(contradictions);
        Assert.Contains(contradictions, c => c.Type == TypeContradiction.DemandeSansProblemeQuantifie);

        var questions = await _client.GetFromJsonAsync<List<QuestionRegistreDto>>($"api/projets/{_projetId}/questions");
        Assert.NotNull(questions);
        Assert.Contains(questions, q => q.Importance == ImportanceQuestion.Bloquante);

        var projet = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");
        Assert.Equal(NiveauMaturite.Niveau1Comprehension, projet!.NiveauMaturite);
    }

    [Fact]
    public async Task DetecterContradictions_appel_repete_ne_duplique_pas_les_questions()
    {
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations",
            new UpsertInformationRegistreDto(null, LibelleDemande, "Je veux une appli mobile", SourceInformation.Declaratif, StatutInformation.Valide));

        await _client.PostAsync($"api/projets/{_projetId}/tracabilite/detecter-contradictions", null);
        await _client.PostAsync($"api/projets/{_projetId}/tracabilite/detecter-contradictions", null);

        var questions = await _client.GetFromJsonAsync<List<QuestionRegistreDto>>($"api/projets/{_projetId}/questions");
        Assert.NotNull(questions);
        Assert.Single(questions);
    }
}

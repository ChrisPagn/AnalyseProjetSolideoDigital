using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Projets;
using Shared.Dtos.Registres;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class RegistresControllerTests : IAsyncLifetime
{
    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _projetId;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();

        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client pour registres", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        var creationProjet = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Projet pour registres", client.Id, null, StatutProjet.EnCours));
        var projet = (await creationProjet.Content.ReadFromJsonAsync<ProjetDto>())!;
        _projetId = projet.Id;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Creer_information_genere_un_code_INF_001()
    {
        var dto = new UpsertInformationRegistreDto(null, "Nom du client", "Cabinet Verdier", SourceInformation.Declaratif, StatutInformation.Valide);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations", dto);
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);

        var information = await reponse.Content.ReadFromJsonAsync<InformationRegistreDto>();
        Assert.Equal("INF-001", information!.Code);
    }

    [Fact]
    public async Task Creer_information_avec_phase_inexistante_retourne_bad_request()
    {
        var dto = new UpsertInformationRegistreDto(999999, "Test", null, SourceInformation.Declaratif, StatutInformation.Inconnu);

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations", dto);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Creer_deux_informations_genere_des_codes_successifs()
    {
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations",
            new UpsertInformationRegistreDto(null, "Info 1", null, SourceInformation.Declaratif, StatutInformation.Inconnu));
        var reponse2 = await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations",
            new UpsertInformationRegistreDto(null, "Info 2", null, SourceInformation.Declaratif, StatutInformation.Inconnu));

        var info2 = await reponse2.Content.ReadFromJsonAsync<InformationRegistreDto>();
        Assert.Equal("INF-002", info2!.Code);
    }

    [Fact]
    public async Task Creer_question_bloquante_ouverte_plafonne_la_maturite_a_1()
    {
        // Bloc A (phases 01-04) terminé pour atteindre Niveau 2 par défaut, afin que le test
        // observe réellement l'effet du plafonnement (redescente 2 -> 1), pas une coïncidence
        // où le niveau était déjà au plafond avant l'ajout de la question.
        var phases = await _client.GetFromJsonAsync<List<Shared.Dtos.Phases.PhaseDto>>($"api/projets/{_projetId}/phases");
        Assert.NotNull(phases);
        foreach (var phase in phases!.Where(p => p.Numero <= 4))
        {
            await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase.Id}/statut",
                new Shared.Dtos.Phases.UpdateStatutPhaseDto(StatutPhase.Terminee));
        }

        var projetAvant = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");
        Assert.Equal(NiveauMaturite.Niveau2AnalyseMetier, projetAvant!.NiveauMaturite);

        await _client.PostAsJsonAsync($"api/projets/{_projetId}/questions",
            new UpsertQuestionRegistreDto(null, "Qui décide en cas de désaccord ?", ImportanceQuestion.Bloquante, StatutQuestion.Ouverte));

        var projetApres = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");
        Assert.Equal(NiveauMaturite.Niveau1Comprehension, projetApres!.NiveauMaturite);
    }

    [Fact]
    public async Task Creer_risque_genere_un_code_R_001()
    {
        var dto = new UpsertRisqueRegistreDto("Dépendance à un unique développeur", "Moyenne", "Élevé", "Documenter le code");

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/risques", dto);

        var risque = await reponse.Content.ReadFromJsonAsync<RisqueRegistreDto>();
        Assert.Equal("R-001", risque!.Code);
    }

    [Fact]
    public async Task Creer_decision_genere_un_code_DEC_001_et_une_date()
    {
        var dto = new UpsertDecisionRegistreDto("Utiliser SQLite en dev", "Simplicité de mise en place", "PostgreSQL écarté pour la V1");

        var reponse = await _client.PostAsJsonAsync($"api/projets/{_projetId}/decisions", dto);

        var decision = await reponse.Content.ReadFromJsonAsync<DecisionRegistreDto>();
        Assert.Equal("DEC-001", decision!.Code);
        Assert.True(decision.Date > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Modifier_information_genere_une_entree_dans_lhistorique()
    {
        var creation = await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations",
            new UpsertInformationRegistreDto(null, "Libellé initial", "Valeur initiale", SourceInformation.Declaratif, StatutInformation.AConfirmer));
        var information = (await creation.Content.ReadFromJsonAsync<InformationRegistreDto>())!;

        await _client.PutAsJsonAsync($"api/projets/{_projetId}/informations/{information.Id}",
            new UpsertInformationRegistreDto(null, "Libellé initial", "Valeur modifiée", SourceInformation.Declaratif, StatutInformation.Valide));

        var historique = await _client.GetFromJsonAsync<List<HistoriqueModificationDto>>(
            $"api/historique?entiteType=InformationRegistre&entiteId={information.Id}");

        Assert.NotNull(historique);
        Assert.Contains(historique, h => h.Champ == "Valeur" && h.AncienneValeur == "Valeur initiale" && h.NouvelleValeur == "Valeur modifiée");
        Assert.Contains(historique, h => h.Champ == "Statut");
    }

    [Fact]
    public async Task Supprimer_risque_reussit()
    {
        var creation = await _client.PostAsJsonAsync($"api/projets/{_projetId}/risques",
            new UpsertRisqueRegistreDto("À supprimer", null, null, null));
        var risque = (await creation.Content.ReadFromJsonAsync<RisqueRegistreDto>())!;

        var reponse = await _client.DeleteAsync($"api/projets/{_projetId}/risques/{risque.Id}");

        Assert.Equal(HttpStatusCode.NoContent, reponse.StatusCode);
    }

    [Fact]
    public async Task GetToutes_informations_filtre_par_phase()
    {
        var phases = await _client.GetFromJsonAsync<List<Shared.Dtos.Phases.PhaseDto>>($"api/projets/{_projetId}/phases");
        Assert.NotNull(phases);
        var phase1 = phases!.First(p => p.Numero == 1);
        var phase2 = phases.First(p => p.Numero == 2);

        await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations",
            new UpsertInformationRegistreDto(phase1.Id, "Info phase 1", null, SourceInformation.Declaratif, StatutInformation.Inconnu));
        await _client.PostAsJsonAsync($"api/projets/{_projetId}/informations",
            new UpsertInformationRegistreDto(phase2.Id, "Info phase 2", null, SourceInformation.Declaratif, StatutInformation.Inconnu));

        var informationsPhase1 = await _client.GetFromJsonAsync<List<InformationRegistreDto>>(
            $"api/projets/{_projetId}/informations?phaseId={phase1.Id}");

        Assert.NotNull(informationsPhase1);
        Assert.Single(informationsPhase1);
        Assert.Equal("Info phase 1", informationsPhase1[0].Libelle);
    }
}

using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Phases;
using Shared.Dtos.Projets;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class PhasesControllerTests : IAsyncLifetime
{
    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _projetId;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();

        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client pour phases", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        var creationProjet = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Projet pour phases", client.Id, null, StatutProjet.EnCours));
        var projet = (await creationProjet.Content.ReadFromJsonAsync<ProjetDto>())!;
        _projetId = projet.Id;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetToutes_retourne_les_18_phases_dans_lordre()
    {
        var phases = await _client.GetFromJsonAsync<List<PhaseDto>>($"api/projets/{_projetId}/phases");

        Assert.NotNull(phases);
        Assert.Equal(18, phases.Count);
        Assert.Equal(Enumerable.Range(1, 18), phases.Select(p => p.Numero));
        Assert.All(phases, p => Assert.Equal(StatutPhase.NonCommencee, p.Statut));
    }

    [Fact]
    public async Task GetToutes_sur_projet_inexistant_retourne_not_found()
    {
        var reponse = await _client.GetAsync("api/projets/999999/phases");

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task GetParNumero_retourne_la_bonne_phase()
    {
        var phase = await _client.GetFromJsonAsync<PhaseDto>($"api/projets/{_projetId}/phases/3");

        Assert.NotNull(phase);
        Assert.Equal(3, phase.Numero);
        Assert.Equal("Processus métier", phase.Nom);
    }

    [Fact]
    public async Task ModifierStatut_phase_inexistante_retourne_not_found()
    {
        var reponse = await _client.PutAsJsonAsync(
            $"api/projets/{_projetId}/phases/999999/statut", new UpdateStatutPhaseDto(StatutPhase.Terminee));

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task ModifierStatut_phases_01_02_terminees_fait_passer_le_projet_au_niveau_1()
    {
        var phases = await _client.GetFromJsonAsync<List<PhaseDto>>($"api/projets/{_projetId}/phases");
        Assert.NotNull(phases);
        var phase1 = phases.First(p => p.Numero == 1);
        var phase2 = phases.First(p => p.Numero == 2);

        await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase1.Id}/statut",
            new UpdateStatutPhaseDto(StatutPhase.Terminee));
        var reponse2 = await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase2.Id}/statut",
            new UpdateStatutPhaseDto(StatutPhase.Terminee));

        var projetMisAJour = await reponse2.Content.ReadFromJsonAsync<ProjetDto>();
        Assert.Equal(NiveauMaturite.Niveau1Comprehension, projetMisAJour!.NiveauMaturite);

        var projetRelu = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{_projetId}");
        Assert.Equal(NiveauMaturite.Niveau1Comprehension, projetRelu!.NiveauMaturite);
    }

    [Fact]
    public async Task ModifierStatut_repasser_une_phase_a_non_commencee_fait_redescendre_la_maturite()
    {
        var phases = await _client.GetFromJsonAsync<List<PhaseDto>>($"api/projets/{_projetId}/phases");
        Assert.NotNull(phases);
        var phase1 = phases.First(p => p.Numero == 1);
        var phase2 = phases.First(p => p.Numero == 2);

        await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase1.Id}/statut",
            new UpdateStatutPhaseDto(StatutPhase.Terminee));
        await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase2.Id}/statut",
            new UpdateStatutPhaseDto(StatutPhase.Terminee));

        var reponseRetour = await _client.PutAsJsonAsync($"api/projets/{_projetId}/phases/{phase2.Id}/statut",
            new UpdateStatutPhaseDto(StatutPhase.NonCommencee));

        var projetMisAJour = await reponseRetour.Content.ReadFromJsonAsync<ProjetDto>();
        Assert.Equal(NiveauMaturite.Niveau0Inconnu, projetMisAJour!.NiveauMaturite);
    }
}

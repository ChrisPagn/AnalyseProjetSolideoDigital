using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Dashboard;
using Shared.Dtos.Projets;
using Shared.Dtos.Registres;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class DashboardControllerTests : IAsyncLifetime
{
    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private async Task<(ClientDto Client, ProjetDto Projet)> CreerProjetAsync(string nomClient, string nomProjet)
    {
        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto(nomClient, null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        var creationProjet = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto(nomProjet, client.Id, null, StatutProjet.EnCours));
        var projet = (await creationProjet.Content.ReadFromJsonAsync<ProjetDto>())!;

        return (client, projet);
    }

    [Fact]
    public async Task Get_retourne_tous_les_projets_avec_leur_maturite()
    {
        var (_, projet) = await CreerProjetAsync("Client Dashboard 1", "Projet Dashboard 1");

        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("api/dashboard");

        Assert.NotNull(dashboard);
        Assert.Contains(dashboard.Projets, p => p.Id == projet.Id && p.NiveauMaturite == NiveauMaturite.Niveau0Inconnu);
    }

    [Fact]
    public async Task Get_compte_les_phases_terminees_par_projet()
    {
        var (_, projet) = await CreerProjetAsync("Client Dashboard 2", "Projet Dashboard 2");

        var phases = await _client.GetFromJsonAsync<List<Shared.Dtos.Phases.PhaseDto>>($"api/projets/{projet.Id}/phases");
        var phase1 = phases!.First(p => p.Numero == 1);
        await _client.PutAsJsonAsync($"api/projets/{projet.Id}/phases/{phase1.Id}/statut",
            new Shared.Dtos.Phases.UpdateStatutPhaseDto(StatutPhase.Terminee));

        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("api/dashboard");

        var resume = dashboard!.Projets.First(p => p.Id == projet.Id);
        Assert.Equal(1, resume.NombrePhasesTerminees);
    }

    [Fact]
    public async Task Get_expose_le_nombre_de_questions_bloquantes_ouvertes_par_projet()
    {
        var (_, projet) = await CreerProjetAsync("Client Dashboard 3", "Projet Dashboard 3");

        await _client.PostAsJsonAsync($"api/projets/{projet.Id}/questions",
            new UpsertQuestionRegistreDto(null, "Qui décide en cas de désaccord ?", ImportanceQuestion.Bloquante, StatutQuestion.Ouverte));
        await _client.PostAsJsonAsync($"api/projets/{projet.Id}/questions",
            new UpsertQuestionRegistreDto(null, "Question normale", ImportanceQuestion.Normale, StatutQuestion.Ouverte));

        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("api/dashboard");

        var resume = dashboard!.Projets.First(p => p.Id == projet.Id);
        Assert.Equal(1, resume.NombreQuestionsBloquantesOuvertes);
    }

    [Fact]
    public async Task Get_liste_les_questions_bloquantes_ouvertes_dans_le_registre_global()
    {
        var (_, projet) = await CreerProjetAsync("Client Dashboard 4", "Projet Dashboard 4");

        await _client.PostAsJsonAsync($"api/projets/{projet.Id}/questions",
            new UpsertQuestionRegistreDto(null, "Question bloquante globale", ImportanceQuestion.Bloquante, StatutQuestion.Ouverte));

        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("api/dashboard");

        Assert.Contains(dashboard!.QuestionsBloquantes,
            q => q.ProjetId == projet.Id && q.Question == "Question bloquante globale" && q.ProjetNom == "Projet Dashboard 4");
    }

    [Fact]
    public async Task Get_nexclut_pas_les_questions_bloquantes_resolues_du_registre() // vérifie l'inverse : elles ne doivent PAS apparaître
    {
        var (_, projet) = await CreerProjetAsync("Client Dashboard 5", "Projet Dashboard 5");

        var creationQuestion = await _client.PostAsJsonAsync($"api/projets/{projet.Id}/questions",
            new UpsertQuestionRegistreDto(null, "Question résolue", ImportanceQuestion.Bloquante, StatutQuestion.Ouverte));
        var question = await creationQuestion.Content.ReadFromJsonAsync<QuestionRegistreDto>();

        await _client.PutAsJsonAsync($"api/projets/{projet.Id}/questions/{question!.Id}",
            new UpsertQuestionRegistreDto(null, "Question résolue", ImportanceQuestion.Bloquante, StatutQuestion.Resolue));

        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("api/dashboard");

        Assert.DoesNotContain(dashboard!.QuestionsBloquantes, q => q.ProjetId == projet.Id);
    }
}

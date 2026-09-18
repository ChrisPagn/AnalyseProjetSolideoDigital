using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Domaine;
using Shared.Dtos.Projets;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class PromptMaitreExportControllerTests : IAsyncLifetime
{
    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _projetId;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();

        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client pour prompt maître", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        var creationProjet = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Projet pour prompt maître", client.Id, "Blazor", StatutProjet.EnCours));
        var projet = (await creationProjet.Content.ReadFromJsonAsync<ProjetDto>())!;
        _projetId = projet.Id;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ExporterPromptMaitre_retourne_le_contenu_en_json()
    {
        var reponse = await _client.GetAsync($"api/projets/{_projetId}/exports/prompt-maitre");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);

        var dto = await reponse.Content.ReadFromJsonAsync<PromptMaitreTransfertDto>();
        Assert.NotNull(dto);
        Assert.Contains("## 1. CONTEXTE DU PROJET", dto.Contenu);
        Assert.Contains("Blazor", dto.Contenu);
    }

    [Fact]
    public async Task ExporterPromptMaitre_sur_projet_inexistant_retourne_not_found()
    {
        var reponse = await _client.GetAsync("api/projets/999999/exports/prompt-maitre");

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }
}

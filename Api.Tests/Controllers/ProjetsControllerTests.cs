using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Projets;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class ProjetsControllerTests : IAsyncLifetime
{
    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _clientId;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();

        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client pour projets", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;
        _clientId = client.Id;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Creer_projet_initialise_18_phases_et_niveau_maturite_zero()
    {
        var dto = new UpsertProjetDto("Projet Test", _clientId, "Blazor", StatutProjet.EnCours);

        var reponse = await _client.PostAsJsonAsync("api/projets", dto);
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);

        var projet = await reponse.Content.ReadFromJsonAsync<ProjetDto>();
        Assert.NotNull(projet);
        Assert.Equal("Projet Test", projet.Nom);
        Assert.Equal(NiveauMaturite.Niveau0Inconnu, projet.NiveauMaturite);
        Assert.Equal("Client pour projets", projet.ClientNom);
    }

    [Fact]
    public async Task Creer_projet_avec_client_inexistant_retourne_bad_request()
    {
        var dto = new UpsertProjetDto("Projet Orphelin", 999999, null, StatutProjet.EnCours);

        var reponse = await _client.PostAsJsonAsync("api/projets", dto);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task GetTous_retourne_le_projet_cree()
    {
        var creation = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Projet Liste", _clientId, null, StatutProjet.EnCours));
        var projet = (await creation.Content.ReadFromJsonAsync<ProjetDto>())!;

        var tous = await _client.GetFromJsonAsync<List<ProjetDto>>("api/projets");

        Assert.Contains(tous!, p => p.Id == projet.Id);
    }

    [Fact]
    public async Task Modifier_projet_persiste_les_changements()
    {
        var creation = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Nom Initial", _clientId, null, StatutProjet.EnCours));
        var projet = (await creation.Content.ReadFromJsonAsync<ProjetDto>())!;

        var reponseModif = await _client.PutAsJsonAsync($"api/projets/{projet.Id}",
            new UpsertProjetDto("Nom Modifié", _clientId, "Laravel", StatutProjet.Suspendu));
        Assert.Equal(HttpStatusCode.NoContent, reponseModif.StatusCode);

        var relu = await _client.GetFromJsonAsync<ProjetDto>($"api/projets/{projet.Id}");
        Assert.Equal("Nom Modifié", relu!.Nom);
        Assert.Equal(StatutProjet.Suspendu, relu.Statut);
    }

    [Fact]
    public async Task Supprimer_projet_reussit()
    {
        var creation = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("À supprimer", _clientId, null, StatutProjet.EnCours));
        var projet = (await creation.Content.ReadFromJsonAsync<ProjetDto>())!;

        var reponseSuppression = await _client.DeleteAsync($"api/projets/{projet.Id}");

        Assert.Equal(HttpStatusCode.NoContent, reponseSuppression.StatusCode);
    }

    [Fact]
    public async Task RecalculerMaturite_sur_projet_inexistant_retourne_not_found()
    {
        var reponse = await _client.PostAsync("api/projets/999999/recalculer-maturite", null);

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }
}

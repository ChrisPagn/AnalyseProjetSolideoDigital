using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Xunit;

namespace Api.Tests.Controllers;

public class ClientsControllerTests : IAsyncLifetime
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

    [Fact]
    public async Task GetTous_sans_authentification_retourne_unauthorized()
    {
        var clientAnonyme = _factory.CreateClient();

        var reponse = await clientAnonyme.GetAsync("api/clients");

        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
    }

    [Fact]
    public async Task Creer_puis_GetTous_retourne_le_client_cree()
    {
        var dto = new UpsertClientDto("Cabinet Test", "Comptabilité", "5 salariés", "Jean Dupont", null, null);

        var reponseCreation = await _client.PostAsJsonAsync("api/clients", dto);
        Assert.Equal(HttpStatusCode.Created, reponseCreation.StatusCode);

        var clientCree = await reponseCreation.Content.ReadFromJsonAsync<ClientDto>();
        Assert.NotNull(clientCree);
        Assert.Equal("Cabinet Test", clientCree.Nom);
        Assert.Equal(0, clientCree.NombreProjets);

        var tous = await _client.GetFromJsonAsync<List<ClientDto>>("api/clients");
        Assert.Contains(tous!, c => c.Id == clientCree.Id);
    }

    [Fact]
    public async Task Creer_avec_nom_vide_retourne_bad_request()
    {
        var dto = new UpsertClientDto("", null, null, null, null, null);

        var reponse = await _client.PostAsJsonAsync("api/clients", dto);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Modifier_client_existant_persiste_les_changements()
    {
        var creation = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Nom Initial", null, null, null, null, null));
        var client = (await creation.Content.ReadFromJsonAsync<ClientDto>())!;

        var reponseModif = await _client.PutAsJsonAsync($"api/clients/{client.Id}",
            new UpsertClientDto("Nom Modifié", "Nouveau secteur", null, null, null, null));
        Assert.Equal(HttpStatusCode.NoContent, reponseModif.StatusCode);

        var relu = await _client.GetFromJsonAsync<ClientDto>($"api/clients/{client.Id}");
        Assert.Equal("Nom Modifié", relu!.Nom);
        Assert.Equal("Nouveau secteur", relu.Secteur);
    }

    [Fact]
    public async Task Modifier_client_inexistant_retourne_not_found()
    {
        var reponse = await _client.PutAsJsonAsync("api/clients/999999",
            new UpsertClientDto("Peu importe", null, null, null, null, null));

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task Supprimer_client_sans_projet_reussit()
    {
        var creation = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("À supprimer", null, null, null, null, null));
        var client = (await creation.Content.ReadFromJsonAsync<ClientDto>())!;

        var reponseSuppression = await _client.DeleteAsync($"api/clients/{client.Id}");

        Assert.Equal(HttpStatusCode.NoContent, reponseSuppression.StatusCode);
    }

    [Fact]
    public async Task Supprimer_client_avec_projet_rattache_retourne_conflict()
    {
        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client Avec Projet", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        await _client.PostAsJsonAsync("api/projets",
            new Shared.Dtos.Projets.UpsertProjetDto("Projet Rattaché", client.Id, null, Shared.Enums.StatutProjet.EnCours));

        var reponseSuppression = await _client.DeleteAsync($"api/clients/{client.Id}");

        Assert.Equal(HttpStatusCode.Conflict, reponseSuppression.StatusCode);
    }
}

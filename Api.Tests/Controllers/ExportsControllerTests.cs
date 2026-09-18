using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Clients;
using Shared.Dtos.Projets;
using Shared.Enums;
using Xunit;

namespace Api.Tests.Controllers;

public class ExportsControllerTests : IAsyncLifetime
{
    private readonly AnalyseProjetWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private int _projetId;

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync();

        var creationClient = await _client.PostAsJsonAsync("api/clients",
            new UpsertClientDto("Client pour export", null, null, null, null, null));
        var client = (await creationClient.Content.ReadFromJsonAsync<ClientDto>())!;

        var creationProjet = await _client.PostAsJsonAsync("api/projets",
            new UpsertProjetDto("Projet pour export", client.Id, null, StatutProjet.EnCours));
        var projet = (await creationProjet.Content.ReadFromJsonAsync<ProjetDto>())!;
        _projetId = projet.Id;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ExporterMarkdown_retourne_un_zip_avec_le_bon_content_type()
    {
        var reponse = await _client.GetAsync($"api/projets/{_projetId}/exports/markdown");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal("application/zip", reponse.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(reponse.Content.Headers.ContentDisposition);
        Assert.EndsWith(".zip", reponse.Content.Headers.ContentDisposition!.FileNameStar ?? reponse.Content.Headers.ContentDisposition.FileName);
    }

    [Fact]
    public async Task ExporterMarkdown_le_zip_contient_18_fichiers_de_phase_plus_ANALYSE()
    {
        var reponse = await _client.GetAsync($"api/projets/{_projetId}/exports/markdown");
        var octets = await reponse.Content.ReadAsByteArrayAsync();

        using var stream = new MemoryStream(octets);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        Assert.Equal(19, archive.Entries.Count);
        Assert.Contains(archive.Entries, e => e.Name == "ANALYSE.md");
        Assert.Contains(archive.Entries, e => e.Name == "01-fiche-client.md");
        Assert.Contains(archive.Entries, e => e.Name.StartsWith("18-"));
    }

    [Fact]
    public async Task ExporterMarkdown_sur_projet_inexistant_retourne_not_found()
    {
        var reponse = await _client.GetAsync("api/projets/999999/exports/markdown");

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }
}

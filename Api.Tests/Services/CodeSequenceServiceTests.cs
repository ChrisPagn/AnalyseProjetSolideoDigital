using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Tests.Services;

public class CodeSequenceServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AnalyseProjetDbContext _db;
    private readonly CodeSequenceService _service;

    public CodeSequenceServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AnalyseProjetDbContext>().UseSqlite(_connection).Options;
        _db = new AnalyseProjetDbContext(options);
        _db.Database.EnsureCreated();
        _service = new CodeSequenceService(_db);
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
    public async Task Premier_code_dun_prefixe_commence_a_001()
    {
        var projetId = await CreerProjetAsync();

        var code = await _service.ProchainCodeAsync(projetId, "INF");

        Assert.Equal("INF-001", code);
    }

    [Fact]
    public async Task Codes_successifs_sincrementent()
    {
        var projetId = await CreerProjetAsync();

        var code1 = await _service.ProchainCodeAsync(projetId, "INF");
        var code2 = await _service.ProchainCodeAsync(projetId, "INF");
        var code3 = await _service.ProchainCodeAsync(projetId, "INF");

        Assert.Equal("INF-001", code1);
        Assert.Equal("INF-002", code2);
        Assert.Equal("INF-003", code3);
    }

    [Fact]
    public async Task Prefixes_differents_ont_des_compteurs_independants()
    {
        var projetId = await CreerProjetAsync();

        var codeInf = await _service.ProchainCodeAsync(projetId, "INF");
        var codeQ = await _service.ProchainCodeAsync(projetId, "Q");

        Assert.Equal("INF-001", codeInf);
        Assert.Equal("Q-001", codeQ);
    }

    [Fact]
    public async Task Projets_differents_ont_des_compteurs_independants()
    {
        var projet1 = await CreerProjetAsync();
        var projet2 = await CreerProjetAsync();

        await _service.ProchainCodeAsync(projet1, "INF");
        await _service.ProchainCodeAsync(projet1, "INF");
        var codeProjet2 = await _service.ProchainCodeAsync(projet2, "INF");

        Assert.Equal("INF-001", codeProjet2);
    }

    [Fact]
    public async Task Numero_jamais_reutilise_meme_apres_suppression_de_la_ligne()
    {
        var projetId = await CreerProjetAsync();

        var code1 = await _service.ProchainCodeAsync(projetId, "INF");
        var information = new InformationRegistre
        {
            Code = code1,
            ProjetId = projetId,
            Libelle = "Test",
            Source = Shared.Enums.SourceInformation.Declaratif
        };
        _db.InformationsRegistre.Add(information);
        await _db.SaveChangesAsync();

        _db.InformationsRegistre.Remove(information);
        await _db.SaveChangesAsync();

        var code2 = await _service.ProchainCodeAsync(projetId, "INF");

        Assert.Equal("INF-002", code2);
    }
}

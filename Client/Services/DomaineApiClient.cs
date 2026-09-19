using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Domaine;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers les 8 entités du domaine projet analysé — aucune logique
/// métier ici (Prompt Maître 5.1).
/// </summary>
public class DomaineApiClient(HttpClient httpClient)
{
    // --- Probleme ---
    public async Task<IReadOnlyList<ProblemeDto>> GetProblemesAsync(int projetId) =>
        await LireAsync<List<ProblemeDto>>($"api/projets/{projetId}/problemes") ?? [];

    public async Task<ProblemeDto?> CreerProblemeAsync(int projetId, UpsertProblemeDto dto) =>
        await EnvoyerEtLireAsync<ProblemeDto>(HttpMethod.Post, $"api/projets/{projetId}/problemes", dto);

    public async Task<bool> ModifierProblemeAsync(int projetId, int id, UpsertProblemeDto dto) =>
        (await EnvoyerAsync(HttpMethod.Put, $"api/projets/{projetId}/problemes/{id}", dto)).IsSuccessStatusCode;

    public async Task<bool> SupprimerProblemeAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/problemes/{id}")).IsSuccessStatusCode;

    // --- Processus / Etapes ---
    public async Task<IReadOnlyList<ProcessusDto>> GetProcessusAsync(int projetId) =>
        await LireAsync<List<ProcessusDto>>($"api/projets/{projetId}/processus") ?? [];

    public async Task<ProcessusDto?> CreerProcessusAsync(int projetId, UpsertProcessusDto dto) =>
        await EnvoyerEtLireAsync<ProcessusDto>(HttpMethod.Post, $"api/projets/{projetId}/processus", dto);

    public async Task<bool> SupprimerProcessusAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/processus/{id}")).IsSuccessStatusCode;

    public async Task<EtapeProcessusDto?> AjouterEtapeAsync(int projetId, int processusId, UpsertEtapeProcessusDto dto) =>
        await EnvoyerEtLireAsync<EtapeProcessusDto>(HttpMethod.Post, $"api/projets/{projetId}/processus/{processusId}/etapes", dto);

    public async Task<bool> SupprimerEtapeAsync(int projetId, int processusId, int etapeId) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/processus/{processusId}/etapes/{etapeId}")).IsSuccessStatusCode;

    // --- Acteur / Permission ---
    public async Task<IReadOnlyList<ActeurDto>> GetActeursAsync(int projetId) =>
        await LireAsync<List<ActeurDto>>($"api/projets/{projetId}/acteurs") ?? [];

    public async Task<ActeurDto?> CreerActeurAsync(int projetId, UpsertActeurDto dto) =>
        await EnvoyerEtLireAsync<ActeurDto>(HttpMethod.Post, $"api/projets/{projetId}/acteurs", dto);

    public async Task<bool> SupprimerActeurAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/acteurs/{id}")).IsSuccessStatusCode;

    public async Task<PermissionDto?> AjouterPermissionAsync(int projetId, int acteurId, UpsertPermissionDto dto) =>
        await EnvoyerEtLireAsync<PermissionDto>(HttpMethod.Post, $"api/projets/{projetId}/acteurs/{acteurId}/permissions", dto);

    public async Task<bool> ModifierPermissionAsync(int projetId, int acteurId, int permissionId, UpsertPermissionDto dto) =>
        (await EnvoyerAsync(HttpMethod.Put, $"api/projets/{projetId}/acteurs/{acteurId}/permissions/{permissionId}", dto)).IsSuccessStatusCode;

    public async Task<bool> SupprimerPermissionAsync(int projetId, int acteurId, int permissionId) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/acteurs/{acteurId}/permissions/{permissionId}")).IsSuccessStatusCode;

    // --- Entite ---
    public async Task<IReadOnlyList<EntiteDto>> GetEntitesAsync(int projetId) =>
        await LireAsync<List<EntiteDto>>($"api/projets/{projetId}/entites") ?? [];

    public async Task<EntiteDto?> CreerEntiteAsync(int projetId, UpsertEntiteDto dto) =>
        await EnvoyerEtLireAsync<EntiteDto>(HttpMethod.Post, $"api/projets/{projetId}/entites", dto);

    public async Task<bool> SupprimerEntiteAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/entites/{id}")).IsSuccessStatusCode;

    // --- DocumentMetier ---
    public async Task<IReadOnlyList<DocumentMetierDto>> GetDocumentsAsync(int projetId) =>
        await LireAsync<List<DocumentMetierDto>>($"api/projets/{projetId}/documents") ?? [];

    public async Task<DocumentMetierDto?> CreerDocumentAsync(int projetId, UpsertDocumentMetierDto dto) =>
        await EnvoyerEtLireAsync<DocumentMetierDto>(HttpMethod.Post, $"api/projets/{projetId}/documents", dto);

    public async Task<bool> SupprimerDocumentAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/documents/{id}")).IsSuccessStatusCode;

    // --- Fonctionnalite / CritereAcceptation ---
    public async Task<IReadOnlyList<FonctionnaliteDto>> GetFonctionnalitesAsync(int projetId) =>
        await LireAsync<List<FonctionnaliteDto>>($"api/projets/{projetId}/fonctionnalites") ?? [];

    public async Task<FonctionnaliteDto?> CreerFonctionnaliteAsync(int projetId, UpsertFonctionnaliteDto dto) =>
        await EnvoyerEtLireAsync<FonctionnaliteDto>(HttpMethod.Post, $"api/projets/{projetId}/fonctionnalites", dto);

    public async Task<bool> SupprimerFonctionnaliteAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/fonctionnalites/{id}")).IsSuccessStatusCode;

    public async Task<CritereAcceptationDto?> AjouterCritereAsync(int projetId, int fonctionnaliteId, UpsertCritereAcceptationDto dto) =>
        await EnvoyerEtLireAsync<CritereAcceptationDto>(HttpMethod.Post, $"api/projets/{projetId}/fonctionnalites/{fonctionnaliteId}/criteres", dto);

    public async Task<bool> SupprimerCritereAsync(int projetId, int fonctionnaliteId, int critereId) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/fonctionnalites/{fonctionnaliteId}/criteres/{critereId}")).IsSuccessStatusCode;

    // --- Automatisation ---
    public async Task<IReadOnlyList<AutomatisationDto>> GetAutomatisationsAsync(int projetId) =>
        await LireAsync<List<AutomatisationDto>>($"api/projets/{projetId}/automatisations") ?? [];

    public async Task<AutomatisationDto?> CreerAutomatisationAsync(int projetId, UpsertAutomatisationDto dto) =>
        await EnvoyerEtLireAsync<AutomatisationDto>(HttpMethod.Post, $"api/projets/{projetId}/automatisations", dto);

    public async Task<bool> SupprimerAutomatisationAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/automatisations/{id}")).IsSuccessStatusCode;

    // --- LienTracabilite ---
    public async Task<IReadOnlyList<LienTracabiliteDto>> GetLiensTracabiliteAsync(int projetId) =>
        await LireAsync<List<LienTracabiliteDto>>($"api/projets/{projetId}/liens-tracabilite") ?? [];

    public async Task<(bool Succes, string? Erreur)> CreerLienTracabiliteAsync(int projetId, CreerLienTracabiliteDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, $"api/projets/{projetId}/liens-tracabilite", dto);
        if (resultat.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, "Impossible de créer le lien : au moins un élément (Problème, Fonctionnalité, Entité ou Critère) est requis.");
    }

    public async Task<bool> SupprimerLienTracabiliteAsync(int projetId, int id) =>
        (await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/liens-tracabilite/{id}")).IsSuccessStatusCode;

    private async Task<T?> LireAsync<T>(string url)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, url);
        return resultat.IsSuccessStatusCode ? await resultat.Content.ReadFromJsonAsync<T>() : default;
    }

    private async Task<T?> EnvoyerEtLireAsync<T>(HttpMethod method, string url, object dto)
    {
        var resultat = await EnvoyerAsync(method, url, dto);
        return resultat.IsSuccessStatusCode ? await resultat.Content.ReadFromJsonAsync<T>() : default;
    }

    private async Task<HttpResponseMessage> EnvoyerAsync(HttpMethod method, string url, object? contenu = null)
    {
        using var requete = new HttpRequestMessage(method, url);
        if (contenu is not null)
        {
            requete.Content = JsonContent.Create(contenu);
        }
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await httpClient.SendAsync(requete);
    }
}

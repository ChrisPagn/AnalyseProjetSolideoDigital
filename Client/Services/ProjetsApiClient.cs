using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Projets;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers /api/projets — aucune logique métier ici (Prompt Maître 5.1).
/// </summary>
public class ProjetsApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ProjetDto>> GetTousAsync()
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, "api/projets");
        return await resultat.Content.ReadFromJsonAsync<List<ProjetDto>>() ?? [];
    }

    public async Task<ProjetDto?> GetParIdAsync(int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, $"api/projets/{id}");
        return resultat.IsSuccessStatusCode
            ? await resultat.Content.ReadFromJsonAsync<ProjetDto>()
            : null;
    }

    public async Task<ProjetDto?> CreerAsync(UpsertProjetDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, "api/projets", dto);
        return resultat.IsSuccessStatusCode
            ? await resultat.Content.ReadFromJsonAsync<ProjetDto>()
            : null;
    }

    public async Task<bool> ModifierAsync(int id, UpsertProjetDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Put, $"api/projets/{id}", dto);
        return resultat.IsSuccessStatusCode;
    }

    public async Task<bool> SupprimerAsync(int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{id}");
        return resultat.IsSuccessStatusCode;
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

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Domaine;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers /api/projets/{projetId}/tracabilite — aucune logique métier ici
/// (Prompt Maître 5.1).
/// </summary>
public class TracabiliteApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AlerteTracabiliteDto>> GetAlertesAsync(int projetId)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, $"api/projets/{projetId}/tracabilite/alertes");
        return resultat.IsSuccessStatusCode
            ? await resultat.Content.ReadFromJsonAsync<List<AlerteTracabiliteDto>>() ?? []
            : [];
    }

    public async Task<IReadOnlyList<ContradictionDto>> DetecterContradictionsAsync(int projetId)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, $"api/projets/{projetId}/tracabilite/detecter-contradictions");
        return resultat.IsSuccessStatusCode
            ? await resultat.Content.ReadFromJsonAsync<List<ContradictionDto>>() ?? []
            : [];
    }

    private async Task<HttpResponseMessage> EnvoyerAsync(HttpMethod method, string url)
    {
        using var requete = new HttpRequestMessage(method, url);
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await httpClient.SendAsync(requete);
    }
}

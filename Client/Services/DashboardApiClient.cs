using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Dashboard;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers /api/dashboard — aucune logique métier ici (Prompt Maître 5.1).
/// </summary>
public class DashboardApiClient(HttpClient httpClient)
{
    public async Task<DashboardDto?> GetAsync()
    {
        using var requete = new HttpRequestMessage(HttpMethod.Get, "api/dashboard");
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var resultat = await httpClient.SendAsync(requete);
        return resultat.IsSuccessStatusCode
            ? await resultat.Content.ReadFromJsonAsync<DashboardDto>()
            : null;
    }
}

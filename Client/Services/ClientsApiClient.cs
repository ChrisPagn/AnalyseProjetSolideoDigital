using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Clients;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers /api/clients — aucune logique métier ici (Prompt Maître 5.1).
/// </summary>
public class ClientsApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ClientDto>> GetTousAsync()
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, "api/clients");
        return await resultat.Content.ReadFromJsonAsync<List<ClientDto>>() ?? [];
    }

    public async Task<ClientDto?> GetParIdAsync(int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, $"api/clients/{id}");
        return resultat.IsSuccessStatusCode
            ? await resultat.Content.ReadFromJsonAsync<ClientDto>()
            : null;
    }

    public async Task<ClientDto?> CreerAsync(UpsertClientDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, "api/clients", dto);
        return resultat.IsSuccessStatusCode
            ? await resultat.Content.ReadFromJsonAsync<ClientDto>()
            : null;
    }

    public async Task<bool> ModifierAsync(int id, UpsertClientDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Put, $"api/clients/{id}", dto);
        return resultat.IsSuccessStatusCode;
    }

    public async Task<(bool Succes, string? Erreur)> SupprimerAsync(int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Delete, $"api/clients/{id}");
        if (resultat.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var erreur = await resultat.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(erreur) ? "Suppression impossible." : erreur);
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

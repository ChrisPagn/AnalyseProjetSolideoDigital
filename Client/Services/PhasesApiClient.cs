using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Phases;
using Shared.Dtos.Projets;
using Shared.Enums;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers /api/projets/{projetId}/phases — aucune logique métier ici
/// (Prompt Maître 5.1).
/// </summary>
public class PhasesApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<PhaseDto>> GetToutesAsync(int projetId)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, $"api/projets/{projetId}/phases");
        return await resultat.Content.ReadFromJsonAsync<List<PhaseDto>>() ?? [];
    }

    public async Task<(bool Succes, ProjetDto? Projet)> ModifierStatutAsync(int projetId, int phaseId, StatutPhase statut)
    {
        var resultat = await EnvoyerAsync(
            HttpMethod.Put, $"api/projets/{projetId}/phases/{phaseId}/statut", new UpdateStatutPhaseDto(statut));

        if (!resultat.IsSuccessStatusCode)
        {
            return (false, null);
        }

        var projet = await resultat.Content.ReadFromJsonAsync<ProjetDto>();
        return (true, projet);
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

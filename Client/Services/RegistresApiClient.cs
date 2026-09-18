using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Registres;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers les 4 registres + historique — aucune logique métier ici
/// (Prompt Maître 5.1).
/// </summary>
public class RegistresApiClient(HttpClient httpClient)
{
    // --- InformationRegistre ---
    public async Task<IReadOnlyList<InformationRegistreDto>> GetInformationsAsync(int projetId, int? phaseId = null)
    {
        var url = phaseId is null
            ? $"api/projets/{projetId}/informations"
            : $"api/projets/{projetId}/informations?phaseId={phaseId}";
        var resultat = await EnvoyerAsync(HttpMethod.Get, url);
        return await resultat.Content.ReadFromJsonAsync<List<InformationRegistreDto>>() ?? [];
    }

    public async Task<InformationRegistreDto?> CreerInformationAsync(int projetId, UpsertInformationRegistreDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, $"api/projets/{projetId}/informations", dto);
        return resultat.IsSuccessStatusCode ? await resultat.Content.ReadFromJsonAsync<InformationRegistreDto>() : null;
    }

    public async Task<bool> ModifierInformationAsync(int projetId, int id, UpsertInformationRegistreDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Put, $"api/projets/{projetId}/informations/{id}", dto);
        return resultat.IsSuccessStatusCode;
    }

    public async Task<bool> SupprimerInformationAsync(int projetId, int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/informations/{id}");
        return resultat.IsSuccessStatusCode;
    }

    // --- QuestionRegistre ---
    public async Task<IReadOnlyList<QuestionRegistreDto>> GetQuestionsAsync(int projetId, int? phaseId = null)
    {
        var url = phaseId is null
            ? $"api/projets/{projetId}/questions"
            : $"api/projets/{projetId}/questions?phaseId={phaseId}";
        var resultat = await EnvoyerAsync(HttpMethod.Get, url);
        return await resultat.Content.ReadFromJsonAsync<List<QuestionRegistreDto>>() ?? [];
    }

    public async Task<QuestionRegistreDto?> CreerQuestionAsync(int projetId, UpsertQuestionRegistreDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, $"api/projets/{projetId}/questions", dto);
        return resultat.IsSuccessStatusCode ? await resultat.Content.ReadFromJsonAsync<QuestionRegistreDto>() : null;
    }

    public async Task<bool> ModifierQuestionAsync(int projetId, int id, UpsertQuestionRegistreDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Put, $"api/projets/{projetId}/questions/{id}", dto);
        return resultat.IsSuccessStatusCode;
    }

    public async Task<bool> SupprimerQuestionAsync(int projetId, int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/questions/{id}");
        return resultat.IsSuccessStatusCode;
    }

    // --- RisqueRegistre ---
    public async Task<IReadOnlyList<RisqueRegistreDto>> GetRisquesAsync(int projetId)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, $"api/projets/{projetId}/risques");
        return await resultat.Content.ReadFromJsonAsync<List<RisqueRegistreDto>>() ?? [];
    }

    public async Task<RisqueRegistreDto?> CreerRisqueAsync(int projetId, UpsertRisqueRegistreDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, $"api/projets/{projetId}/risques", dto);
        return resultat.IsSuccessStatusCode ? await resultat.Content.ReadFromJsonAsync<RisqueRegistreDto>() : null;
    }

    public async Task<bool> SupprimerRisqueAsync(int projetId, int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/risques/{id}");
        return resultat.IsSuccessStatusCode;
    }

    // --- DecisionRegistre ---
    public async Task<IReadOnlyList<DecisionRegistreDto>> GetDecisionsAsync(int projetId)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, $"api/projets/{projetId}/decisions");
        return await resultat.Content.ReadFromJsonAsync<List<DecisionRegistreDto>>() ?? [];
    }

    public async Task<DecisionRegistreDto?> CreerDecisionAsync(int projetId, UpsertDecisionRegistreDto dto)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Post, $"api/projets/{projetId}/decisions", dto);
        return resultat.IsSuccessStatusCode ? await resultat.Content.ReadFromJsonAsync<DecisionRegistreDto>() : null;
    }

    public async Task<bool> SupprimerDecisionAsync(int projetId, int id)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Delete, $"api/projets/{projetId}/decisions/{id}");
        return resultat.IsSuccessStatusCode;
    }

    // --- Historique ---
    public async Task<IReadOnlyList<HistoriqueModificationDto>> GetHistoriqueAsync(string entiteType, int entiteId)
    {
        var resultat = await EnvoyerAsync(HttpMethod.Get, $"api/historique?entiteType={entiteType}&entiteId={entiteId}");
        return await resultat.Content.ReadFromJsonAsync<List<HistoriqueModificationDto>>() ?? [];
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

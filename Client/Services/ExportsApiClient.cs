using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Shared.Dtos.Domaine;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers /api/projets/{projetId}/exports — aucune logique métier ici
/// (Prompt Maître 5.1). Déclenche le téléchargement du fichier via interop JS (pas d'écriture
/// sur disque côté serveur — décision validée avec l'utilisateur, étape 9).
/// </summary>
public class ExportsApiClient(HttpClient httpClient, IJSRuntime jsRuntime)
{
    public async Task<bool> TelechargerMarkdownAsync(int projetId)
    {
        using var requete = new HttpRequestMessage(HttpMethod.Get, $"api/projets/{projetId}/exports/markdown");
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var reponse = await httpClient.SendAsync(requete);
        if (!reponse.IsSuccessStatusCode)
        {
            return false;
        }

        var octets = await reponse.Content.ReadAsByteArrayAsync();
        var nomFichier = reponse.Content.Headers.ContentDisposition?.FileNameStar
            ?? reponse.Content.Headers.ContentDisposition?.FileName
            ?? "export.zip";

        await jsRuntime.InvokeVoidAsync("analyseProjetDownload.telechargerFichier", nomFichier, octets);
        return true;
    }

    public async Task<string?> GenererPromptMaitreAsync(int projetId)
    {
        using var requete = new HttpRequestMessage(HttpMethod.Get, $"api/projets/{projetId}/exports/prompt-maitre");
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var reponse = await httpClient.SendAsync(requete);
        if (!reponse.IsSuccessStatusCode)
        {
            return null;
        }

        var dto = await reponse.Content.ReadFromJsonAsync<PromptMaitreTransfertDto>();
        return dto?.Contenu;
    }

    public async Task CopierDansPressePapierAsync(string texte)
    {
        await jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", texte);
    }
}

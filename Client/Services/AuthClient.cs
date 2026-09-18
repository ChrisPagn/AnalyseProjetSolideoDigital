using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos.Auth;

namespace Client.Services;

/// <summary>
/// Wrapper HttpClient typé vers les endpoints /api/auth — aucune logique métier ici,
/// seulement l'appel HTTP (Prompt Maître 5.1 : le Client ne fait qu'orchestrer des appels).
/// BrowserRequestCredentials.Include : nécessaire pour que le cookie Identity voyage même
/// quand l'Api est sur une origine différente (dev local, voir Program.cs) ; sans effet quand
/// Client et Api partagent la même origine (production, Prompt Maître 7.3).
/// </summary>
public class AuthClient(HttpClient httpClient)
{
    public async Task<(bool Succes, string? Erreur)> LoginAsync(string email, string password)
    {
        using var requete = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequestDto(email, password))
        };
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var reponse = await httpClient.SendAsync(requete);

        if (reponse.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var message = reponse.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Identifiants invalides.",
            System.Net.HttpStatusCode.Locked => "Compte temporairement bloqué suite à plusieurs tentatives échouées.",
            System.Net.HttpStatusCode.TooManyRequests => "Trop de tentatives, réessayez dans une minute.",
            _ => "Erreur de connexion, réessayez."
        };

        return (false, message);
    }

    public async Task LogoutAsync()
    {
        using var requete = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        await httpClient.SendAsync(requete);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync()
    {
        using var requete = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        requete.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var reponse = await httpClient.SendAsync(requete);
        var utilisateur = await reponse.Content.ReadFromJsonAsync<CurrentUserDto>();
        return utilisateur ?? new CurrentUserDto(false, null);
    }
}

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Dtos.Auth;

namespace Api.Tests;

public static class AuthenticatedClientExtensions
{
    /// <summary>
    /// Crée un HttpClient de test déjà connecté (cookie Identity conservé entre requêtes) —
    /// utilisé par tous les tests de controllers protégés par [Authorize].
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(this AnalyseProjetWebApplicationFactory factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var reponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequestDto(
            AnalyseProjetWebApplicationFactory.AdminEmailTest,
            AnalyseProjetWebApplicationFactory.AdminPasswordTest));

        reponse.EnsureSuccessStatusCode();

        return client;
    }
}

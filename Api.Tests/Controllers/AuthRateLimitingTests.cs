using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Auth;
using Xunit;

namespace Api.Tests.Controllers;

/// <summary>
/// Isolé dans sa propre factory (pas IClassFixture partagé) : le rate limiter et le lockout de
/// compte sont tous les deux stateful, un test qui les déclenche ne doit pas fausser les autres
/// tests de AuthControllerTests.
/// </summary>
public class AuthRateLimitingTests
{
    [Fact]
    public async Task Login_repete_au_dela_de_la_limite_retourne_too_many_requests()
    {
        using var factory = new AnalyseProjetWebApplicationFactory();
        var client = factory.CreateClient();
        var requete = new LoginRequestDto("quelquun@exemple.fr", "MotDePasseInvalide123!");

        HttpResponseMessage? derniereReponse = null;

        // La politique "login" autorise 5 requêtes par minute et par IP (Program.cs) ;
        // la 6e doit être rejetée par le rate limiter avant même d'atteindre le lockout Identity.
        for (var i = 0; i < 6; i++)
        {
            derniereReponse = await client.PostAsJsonAsync("api/auth/login", requete);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, derniereReponse!.StatusCode);
    }
}

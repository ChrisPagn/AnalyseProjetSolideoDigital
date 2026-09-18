using System.Net;
using System.Net.Http.Json;
using Shared.Dtos.Auth;
using Xunit;

namespace Api.Tests.Controllers;

public class AuthControllerTests : IClassFixture<AnalyseProjetWebApplicationFactory>
{
    private readonly AnalyseProjetWebApplicationFactory _factory;

    public AuthControllerTests(AnalyseProjetWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Me_sans_authentification_retourne_non_authentifie()
    {
        var client = _factory.CreateClient();

        var reponse = await client.GetFromJsonAsync<CurrentUserDto>("api/auth/me");

        Assert.NotNull(reponse);
        Assert.False(reponse.EstAuthentifie);
    }

    [Fact]
    public async Task Login_avec_identifiants_corrects_retourne_ok_et_pose_un_cookie()
    {
        var client = _factory.CreateClient();

        var reponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequestDto(
            AnalyseProjetWebApplicationFactory.AdminEmailTest,
            AnalyseProjetWebApplicationFactory.AdminPasswordTest));

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.True(reponse.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Login_avec_mauvais_mot_de_passe_retourne_unauthorized()
    {
        var client = _factory.CreateClient();

        var reponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequestDto(
            AnalyseProjetWebApplicationFactory.AdminEmailTest,
            "MauvaisMotDePasse123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
    }

    [Fact]
    public async Task Login_avec_email_inexistant_retourne_unauthorized_sans_reveler_labsence_du_compte()
    {
        var client = _factory.CreateClient();

        var reponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequestDto(
            "personne@nexiste-pas.fr",
            "PeuImporteLeMotDePasse123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
    }

    [Fact]
    public async Task Login_authentifie_puis_me_retourne_utilisateur_authentifie()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        await client.PostAsJsonAsync("api/auth/login", new LoginRequestDto(
            AnalyseProjetWebApplicationFactory.AdminEmailTest,
            AnalyseProjetWebApplicationFactory.AdminPasswordTest));

        var utilisateur = await client.GetFromJsonAsync<CurrentUserDto>("api/auth/me");

        Assert.NotNull(utilisateur);
        Assert.True(utilisateur.EstAuthentifie);
        Assert.Equal(AnalyseProjetWebApplicationFactory.AdminEmailTest, utilisateur.Email);
    }

    [Fact]
    public async Task Logout_sans_authentification_prealable_retourne_unauthorized()
    {
        var client = _factory.CreateClient();

        var reponse = await client.PostAsync("api/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
    }

    [Fact]
    public async Task Login_avec_dto_invalide_retourne_bad_request()
    {
        var client = _factory.CreateClient();

        var reponse = await client.PostAsJsonAsync("api/auth/login", new LoginRequestDto("pas-un-email", ""));

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Services;

/// <summary>
/// Détermine l'état d'authentification en interrogeant /api/auth/me (cookie Identity envoyé
/// automatiquement par le navigateur, même origine que l'Api — Prompt Maître 7.3).
/// </summary>
public class CookieAuthenticationStateProvider(AuthClient authClient) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonyme = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var utilisateur = await authClient.GetCurrentUserAsync();

        if (!utilisateur.EstAuthentifie || utilisateur.Email is null)
        {
            return new AuthenticationState(Anonyme);
        }

        var identite = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, utilisateur.Email), new Claim(ClaimTypes.Email, utilisateur.Email)],
            authenticationType: "cookie");

        return new AuthenticationState(new ClaimsPrincipal(identite));
    }

    public void NotifierChangementEtat()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

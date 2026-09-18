using System.Security.Claims;

namespace Api.Services;

public class HttpUtilisateurCourantAccessor(IHttpContextAccessor httpContextAccessor) : IUtilisateurCourantAccessor
{
    public string ObtenirIdentifiant() =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ?? "système";
}

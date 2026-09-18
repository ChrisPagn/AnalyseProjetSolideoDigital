namespace Api.Services;

/// <summary>
/// Abstraction pour obtenir l'identité de l'utilisateur courant sans coupler le DbContext à
/// IHttpContextAccessor (utile aussi pour le seed, qui tourne hors requête HTTP).
/// </summary>
public interface IUtilisateurCourantAccessor
{
    string ObtenirIdentifiant();
}

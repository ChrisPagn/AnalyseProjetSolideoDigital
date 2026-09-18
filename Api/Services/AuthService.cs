using Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos.Auth;

namespace Api.Services;

public enum ResultatConnexion
{
    Succes,
    IdentifiantsInvalides,
    CompteBloque
}

public class AuthService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager)
{
    public async Task<ResultatConnexion> ConnecterAsync(LoginRequestDto dto)
    {
        var utilisateur = await userManager.FindByEmailAsync(dto.Email);
        if (utilisateur is null)
        {
            // On exécute quand même une vérification de mot de passe factice pour éviter
            // qu'un timing attack ne révèle si l'e-mail existe en base.
            await signInManager.CheckPasswordSignInAsync(new ApplicationUser(), dto.Password, lockoutOnFailure: true);
            return ResultatConnexion.IdentifiantsInvalides;
        }

        var resultat = await signInManager.PasswordSignInAsync(
            utilisateur,
            dto.Password,
            isPersistent: true,
            lockoutOnFailure: true);

        if (resultat.IsLockedOut)
        {
            return ResultatConnexion.CompteBloque;
        }

        return resultat.Succeeded ? ResultatConnexion.Succes : ResultatConnexion.IdentifiantsInvalides;
    }

    public Task DeconnecterAsync() => signInManager.SignOutAsync();
}

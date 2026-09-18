using Api.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Data;

/// <summary>
/// Crée le compte admin unique (Prompt Maître section 9 : mono-utilisateur) au démarrage s'il
/// n'existe pas encore. Les identifiants viennent exclusivement de la configuration
/// (variables d'environnement / user-secrets en dev) — jamais hardcodés dans le code
/// (interdiction absolue, Prompt Maître section 14).
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var email = configuration["AdminAccount:Email"];
        var password = configuration["AdminAccount:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "AdminAccount:Email / AdminAccount:Password absents de la configuration — " +
                "aucun compte admin créé. Définir ces valeurs (variables d'environnement " +
                "AdminAccount__Email / AdminAccount__Password, ou user-secrets en dev) avant de " +
                "pouvoir se connecter.");
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var resultat = await userManager.CreateAsync(admin, password);

        if (!resultat.Succeeded)
        {
            var erreurs = string.Join("; ", resultat.Errors.Select(e => e.Description));
            logger.LogError("Échec de création du compte admin : {Erreurs}", erreurs);
        }
    }
}

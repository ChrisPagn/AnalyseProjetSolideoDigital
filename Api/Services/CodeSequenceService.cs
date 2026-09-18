using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

/// <summary>
/// Génère les codes auto-incrémentés par préfixe et par projet (INF-001, Q-001, R-001, DEC-001,
/// etc. — Prompt Maître 4.3). Un compteur dédié (CompteurCode) garantit qu'un numéro n'est jamais
/// réutilisé, même après suppression de la ligne qui le portait.
/// </summary>
public class CodeSequenceService(AnalyseProjetDbContext db)
{
    public async Task<string> ProchainCodeAsync(int projetId, string prefixe, CancellationToken cancellationToken = default)
    {
        var compteur = await db.CompteursCode.FirstOrDefaultAsync(
            c => c.ProjetId == projetId && c.Prefixe == prefixe, cancellationToken);

        if (compteur is null)
        {
            compteur = new CompteurCode { ProjetId = projetId, Prefixe = prefixe, DernierNumero = 0 };
            db.CompteursCode.Add(compteur);
        }

        compteur.DernierNumero++;
        await db.SaveChangesAsync(cancellationToken);

        return $"{prefixe}-{compteur.DernierNumero:D3}";
    }
}

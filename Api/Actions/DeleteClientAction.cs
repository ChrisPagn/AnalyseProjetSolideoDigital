using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Actions;

/// <summary>
/// Un Client avec des Projets rattachés ne peut pas être supprimé (Projet.ClientId est en
/// DeleteBehavior.Restrict, AnalyseProjetDbContext) — cette action donne un message métier clair
/// plutôt que de laisser remonter l'exception SQL brute au Controller.
/// </summary>
public class DeleteClientAction(AnalyseProjetDbContext db)
{
    public async Task<ResultatSuppression> ExecuterAsync(int clientId, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);
        if (client is null)
        {
            return ResultatSuppression.Introuvable;
        }

        var aDesProjets = await db.Projets.AnyAsync(p => p.ClientId == clientId, cancellationToken);
        if (aDesProjets)
        {
            return ResultatSuppression.Bloque;
        }

        db.Clients.Remove(client);
        await db.SaveChangesAsync(cancellationToken);
        return ResultatSuppression.Succes;
    }
}

public enum ResultatSuppression
{
    Succes,
    Introuvable,
    Bloque
}

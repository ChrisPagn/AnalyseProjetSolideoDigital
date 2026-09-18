using Api.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;

namespace Api.Services;

/// <summary>
/// Détecte les liens structurels manquants (rien ne relie A à B) — Prompt Maître 4.3.
/// Distinct de ContradictionDetectorService, qui détecte des incohérences sémantiques entre
/// deux informations déjà déclarées.
/// </summary>
public class TracabiliteService(AnalyseProjetDbContext db)
{
    public async Task<IReadOnlyList<AlerteTracabiliteDto>> DetecterOrphelinsAsync(
        int projetId, CancellationToken cancellationToken = default)
    {
        var alertes = new List<AlerteTracabiliteDto>();

        var fonctionnalitesOrphelines = await db.Fonctionnalites
            .Where(f => f.ProjetId == projetId && f.LiensTracabilite.Count == 0)
            .Select(f => new { f.Id, f.Code, f.Nom })
            .ToListAsync(cancellationToken);

        alertes.AddRange(fonctionnalitesOrphelines.Select(f => new AlerteTracabiliteDto(
            TypeAlerteTracabilite.FonctionnaliteOrpheline,
            f.Id,
            f.Code,
            $"Fonctionnalité orpheline : « {f.Nom} » ({f.Code}) n'est reliée à aucun besoin.")));

        var problemesNonCouverts = await db.Problemes
            .Where(p => p.ProjetId == projetId && p.LiensTracabilite.Count == 0)
            .Select(p => new { p.Id, p.Code, p.Description })
            .ToListAsync(cancellationToken);

        alertes.AddRange(problemesNonCouverts.Select(p => new AlerteTracabiliteDto(
            TypeAlerteTracabilite.BesoinNonCouvert,
            p.Id,
            p.Code,
            $"Besoin non couvert : « {p.Description} » ({p.Code}) n'a aucune fonctionnalité qui le couvre.")));

        return alertes;
    }
}

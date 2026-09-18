using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Api.Actions;

/// <summary>
/// Changer le statut d'une Phase peut faire varier NiveauParPhases (Prompt Maître 4.3) : le
/// recalcul de NiveauMaturite se fait dans la même transaction EF Core explicite (section 5.4).
/// </summary>
public class UpdateStatutPhaseAction(AnalyseProjetDbContext db, MaturiteCalculatorService maturiteCalculator)
{
    public async Task<ResultatMajStatutPhase> ExecuterAsync(
        int projetId, int phaseId, StatutPhase nouveauStatut, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var phase = await db.Phases.FirstOrDefaultAsync(
            p => p.Id == phaseId && p.ProjetId == projetId, cancellationToken);

        if (phase is null)
        {
            return ResultatMajStatutPhase.Introuvable;
        }

        phase.Statut = nouveauStatut;
        phase.DateMaj = DateTime.UtcNow;

        // Le nouveau statut doit être persisté avant le recalcul : MaturiteCalculatorService
        // relit les Phases par une requête SQL directe (db.Phases.Where(...).ToListAsync), qui ne
        // verrait pas ce changement s'il restait seulement suivi en mémoire par EF Core.
        await db.SaveChangesAsync(cancellationToken);

        await maturiteCalculator.RecalculerEtPersisterAsync(projetId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return ResultatMajStatutPhase.Succes;
    }
}

public enum ResultatMajStatutPhase
{
    Succes,
    Introuvable
}

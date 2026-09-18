using Api.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Api.Services;

/// <summary>
/// Calcule NiveauMaturite = MIN(NiveauParPhases, NiveauMaxAutorise) — Prompt Maître 4.3.
/// Règle non contournable (section 14) : ce service est le SEUL endroit qui doit écrire
/// Projet.NiveauMaturite ; aucun controller ni composant Blazor ne doit l'assigner directement.
/// </summary>
public class MaturiteCalculatorService(AnalyseProjetDbContext db)
{
    public async Task<NiveauMaturite> CalculerAsync(int projetId, CancellationToken cancellationToken = default)
    {
        var niveauParPhases = await CalculerNiveauParPhasesAsync(projetId, cancellationToken);
        var niveauMaxAutorise = await CalculerNiveauMaxAutoriseAsync(projetId, cancellationToken);

        var niveau = Math.Min(niveauParPhases, niveauMaxAutorise);
        return (NiveauMaturite)niveau;
    }

    /// <summary>
    /// Recalcule et persiste le niveau de maturité du projet. À appeler dans la même transaction
    /// que toute opération qui modifie une Phase, une QuestionRegistre, une Fonctionnalite, un
    /// Probleme ou un LienTracabilite (Prompt Maître 5.4).
    /// </summary>
    public async Task<NiveauMaturite> RecalculerEtPersisterAsync(int projetId, CancellationToken cancellationToken = default)
    {
        var projet = await db.Projets.FirstAsync(p => p.Id == projetId, cancellationToken);
        var niveau = await CalculerAsync(projetId, cancellationToken);
        projet.NiveauMaturite = niveau;
        return niveau;
    }

    /// <summary>
    /// Paliers du Bloc A (seul bloc figé dans le Prompt Maître à ce jour, section 4.3) :
    /// 0 phase Terminée → 0 ; Phases 01-02 Terminées → 1 ; Phases 01-04 Terminées → 2.
    /// Les seuils des Blocs B/C/D/E ne sont pas encore définis dans le Prompt Maître : le niveau
    /// est plafonné à 2 tant qu'ils ne le seront pas, même si davantage de phases sont terminées.
    /// Niveau 5 (Phases 01-18 Terminées, zéro orphelin) reste atteignable indépendamment de ce
    /// plafond intermédiaire, car il ne dépend que du nombre total de phases terminées.
    /// </summary>
    private async Task<int> CalculerNiveauParPhasesAsync(int projetId, CancellationToken cancellationToken)
    {
        var phases = await db.Phases
            .Where(p => p.ProjetId == projetId)
            .Select(p => new { p.Numero, p.Statut })
            .ToListAsync(cancellationToken);

        var terminees = phases.Where(p => p.Statut == StatutPhase.Terminee).Select(p => p.Numero).ToHashSet();

        if (phases.Count > 0 && phases.Count == terminees.Count && Enumerable.Range(1, 18).All(terminees.Contains))
        {
            return 5;
        }

        var blocATermine = Enumerable.Range(1, 4).All(terminees.Contains);
        if (blocATermine)
        {
            return 2;
        }

        var debutBlocATermine = Enumerable.Range(1, 2).All(terminees.Contains);
        if (debutBlocATermine)
        {
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// 1 si au moins une QuestionRegistre Bloquante Ouverte ; 4 si aucune bloquante ouverte mais
    /// au moins une Fonctionnalite orpheline (sans LienTracabilite) ou un Probleme non couvert ;
    /// 5 sinon. La couverture d'un Probleme se lit exclusivement via LienTracabilite (Prompt
    /// Maître 4.1/4.3/14) — jamais via un champ texte libre.
    /// </summary>
    private async Task<int> CalculerNiveauMaxAutoriseAsync(int projetId, CancellationToken cancellationToken)
    {
        var questionBloquanteOuverte = await db.QuestionsRegistre.AnyAsync(
            q => q.ProjetId == projetId
                 && q.Importance == ImportanceQuestion.Bloquante
                 && q.Statut == StatutQuestion.Ouverte,
            cancellationToken);

        if (questionBloquanteOuverte)
        {
            return 1;
        }

        var fonctionnaliteOrpheline = await db.Fonctionnalites.AnyAsync(
            f => f.ProjetId == projetId && f.LiensTracabilite.Count == 0,
            cancellationToken);

        var problemeNonCouvert = await db.Problemes.AnyAsync(
            p => p.ProjetId == projetId && p.LiensTracabilite.Count == 0,
            cancellationToken);

        if (fonctionnaliteOrpheline || problemeNonCouvert)
        {
            return 4;
        }

        return 5;
    }
}

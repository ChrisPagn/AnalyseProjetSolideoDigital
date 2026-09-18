using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Domaine;
using Shared.Enums;
using Shared.ModeEntretien;

namespace Api.Services;

/// <summary>
/// Détecte des incohérences sémantiques entre deux informations déjà déclarées (Prompt Maître
/// 4.3) — distinct de TracabiliteService, qui détecte des liens structurels manquants.
/// Couvre les 3 cas listés dans le Prompt Maître : (1) Demande (Phase 02) vs Problème réel
/// quantifié (Phase 04) non explicitement actée comme divergence assumée ; (2) deux
/// InformationRegistre au même libellé normalisé avec des valeurs contradictoires ; (3) une
/// EtapeProcessus non validée alors que la Phase 03 est marquée Terminee.
/// Chaque contradiction détectée génère automatiquement une QuestionRegistre de relance
/// (pas seulement un signalement passif) — la génération est idempotente : si une question
/// ouverte avec le même texte existe déjà, elle n'est pas dupliquée.
/// </summary>
public class ContradictionDetectorService(AnalyseProjetDbContext db, CodeSequenceService codeSequence)
{
    /// <summary>Libellé exact de la question guidée "Demande" (Phase 02, Shared/ModeEntretien).</summary>
    private static readonly string LibelleDemande =
        QuestionsGuideesParPhase.Phase02DecouverteProjet.Single(q => q.Numero == 3).Libelle;

    public async Task<IReadOnlyList<ContradictionDto>> DetecterEtGenererRelancesAsync(
        int projetId, CancellationToken cancellationToken = default)
    {
        var contradictions = new List<ContradictionDto>();

        contradictions.AddRange(await DetecterDemandeSansProblemeQuantifieAsync(projetId, cancellationToken));
        contradictions.AddRange(await DetecterInformationsContradictoiresAsync(projetId, cancellationToken));
        contradictions.AddRange(await DetecterEtapesNonvalideesSurPhaseTermineeAsync(projetId, cancellationToken));

        if (contradictions.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return contradictions;
    }

    /// <summary>
    /// Cas 1 : la Demande (Phase 02) est validée mais aucun Probleme quantifié (ScoreCalcule > 0)
    /// n'existe encore pour ce projet — détection structurelle simple, pas d'analyse sémantique du
    /// texte (décision validée avec l'utilisateur).
    /// </summary>
    private async Task<List<ContradictionDto>> DetecterDemandeSansProblemeQuantifieAsync(
        int projetId, CancellationToken cancellationToken)
    {
        var demandeValidee = await db.InformationsRegistre.AnyAsync(
            i => i.ProjetId == projetId && i.Libelle == LibelleDemande && i.Statut == StatutInformation.Valide,
            cancellationToken);

        if (!demandeValidee)
        {
            return [];
        }

        var problemes = await db.Problemes
            .Where(p => p.ProjetId == projetId)
            .Select(p => p.ScoreCalcule)
            .ToListAsync(cancellationToken);

        var auMoinsUnProblemeQuantifie = problemes.Any(score => score > 0);
        if (auMoinsUnProblemeQuantifie)
        {
            return [];
        }

        const string texteQuestion =
            "La demande a été validée en Phase 02 mais aucun problème quantifié n'existe encore en " +
            "Phase 04 — confirmer le besoin réel derrière la demande, ou acter explicitement la divergence.";

        await CreerQuestionRelanceSiAbsenteAsync(projetId, texteQuestion, ImportanceQuestion.Bloquante, cancellationToken);

        return [new ContradictionDto(TypeContradiction.DemandeSansProblemeQuantifie, texteQuestion, null)];
    }

    /// <summary>
    /// Cas 2 : deux InformationRegistre au même libellé normalisé (casse + espaces ignorés) avec
    /// des valeurs non vides et textuellement différentes.
    /// </summary>
    private async Task<List<ContradictionDto>> DetecterInformationsContradictoiresAsync(
        int projetId, CancellationToken cancellationToken)
    {
        var informations = await db.InformationsRegistre
            .Where(i => i.ProjetId == projetId && i.Valeur != null && i.Valeur != "")
            .Select(i => new { i.Code, i.Libelle, i.Valeur })
            .ToListAsync(cancellationToken);

        var contradictions = new List<ContradictionDto>();

        var groupes = informations.GroupBy(i => NormaliserLibelle(i.Libelle));
        foreach (var groupe in groupes)
        {
            var valeursDistinctes = groupe.Select(i => i.Valeur!.Trim()).Distinct().ToList();
            if (valeursDistinctes.Count <= 1)
            {
                continue;
            }

            var libelleOriginal = groupe.First().Libelle;
            var codes = string.Join(", ", groupe.Select(i => i.Code));
            var texteQuestion =
                $"Informations contradictoires sur « {libelleOriginal} » ({codes}) : valeurs différentes " +
                $"déclarées ({string.Join(" vs ", valeursDistinctes)}) — à clarifier.";

            await CreerQuestionRelanceSiAbsenteAsync(projetId, texteQuestion, ImportanceQuestion.Normale, cancellationToken);
            contradictions.Add(new ContradictionDto(TypeContradiction.InformationsContradictoires, texteQuestion, null));
        }

        return contradictions;
    }

    /// <summary>
    /// Cas 3 : une EtapeProcessus a ExempleValide = false alors que la Phase 03 (Processus
    /// métier) est marquée Terminee.
    /// </summary>
    private async Task<List<ContradictionDto>> DetecterEtapesNonvalideesSurPhaseTermineeAsync(
        int projetId, CancellationToken cancellationToken)
    {
        var phase03Terminee = await db.Phases.AnyAsync(
            p => p.ProjetId == projetId && p.Numero == 3 && p.Statut == StatutPhase.Terminee,
            cancellationToken);

        if (!phase03Terminee)
        {
            return [];
        }

        var etapesNonValidees = await db.EtapesProcessus
            .Where(e => e.Processus!.ProjetId == projetId && !e.ExempleValide)
            .Select(e => new { e.Processus!.Code, e.Ordre, e.Action })
            .ToListAsync(cancellationToken);

        var contradictions = new List<ContradictionDto>();

        foreach (var etape in etapesNonValidees)
        {
            var texteQuestion =
                $"La Phase 03 (Processus métier) est marquée Terminée mais l'étape {etape.Ordre} " +
                $"« {etape.Action} » du processus {etape.Code} n'a pas d'exemple concret vérifié " +
                "(ExempleValide = false) — confirmer avec un exemple réel avant de considérer la phase close.";

            await CreerQuestionRelanceSiAbsenteAsync(projetId, texteQuestion, ImportanceQuestion.Normale, cancellationToken);
            contradictions.Add(new ContradictionDto(TypeContradiction.EtapeNonValideeSurPhaseTerminee, texteQuestion, null));
        }

        return contradictions;
    }

    private async Task CreerQuestionRelanceSiAbsenteAsync(
        int projetId, string texteQuestion, ImportanceQuestion importance, CancellationToken cancellationToken)
    {
        var existeDeja = await db.QuestionsRegistre.AnyAsync(
            q => q.ProjetId == projetId && q.Question == texteQuestion && q.Statut == StatutQuestion.Ouverte,
            cancellationToken);

        if (existeDeja)
        {
            return;
        }

        var code = await codeSequence.ProchainCodeAsync(projetId, "Q", cancellationToken);

        db.QuestionsRegistre.Add(new QuestionRegistre
        {
            Code = code,
            ProjetId = projetId,
            Question = texteQuestion,
            Importance = importance,
            Statut = StatutQuestion.Ouverte
        });
    }

    private static string NormaliserLibelle(string libelle) =>
        string.Join(' ', libelle.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}

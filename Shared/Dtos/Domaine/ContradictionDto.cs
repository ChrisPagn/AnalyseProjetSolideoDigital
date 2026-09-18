namespace Shared.Dtos.Domaine;

/// <summary>
/// Incohérence sémantique entre deux informations déjà déclarées (Prompt Maître 4.3,
/// ContradictionDetectorService) — distincte d'une alerte de traçabilité (lien structurel
/// manquant). Chaque contradiction détectée génère automatiquement une QuestionRegistre de
/// relance plutôt que d'être seulement signalée passivement.
/// </summary>
public record ContradictionDto(TypeContradiction Type, string Message, string? CodeQuestionRelance);

public enum TypeContradiction
{
    DemandeSansProblemeQuantifie,
    InformationsContradictoires,
    EtapeNonValideeSurPhaseTerminee
}

namespace Shared.Dtos.Domaine;

/// <summary>
/// Alerte structurelle : un lien manque entre deux éléments qui devraient être reliés
/// (Prompt Maître 4.3, TracabiliteService). À distinguer d'une contradiction (incohérence
/// sémantique entre deux informations déjà déclarées, ContradictionDetectorService).
/// </summary>
public record AlerteTracabiliteDto(TypeAlerteTracabilite Type, int EntiteId, string EntiteCode, string Message);

public enum TypeAlerteTracabilite
{
    FonctionnaliteOrpheline,
    BesoinNonCouvert
}

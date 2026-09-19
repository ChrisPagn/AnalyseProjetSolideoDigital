namespace Shared.Dtos.Domaine;

public record LienTracabiliteDto(
    int Id,
    int? ProblemeId,
    string? ProblemeCode,
    int? FonctionnaliteId,
    string? FonctionnaliteCode,
    int? EntiteId,
    string? EntiteCode,
    int? CritereAcceptationId,
    int? InformationRegistreId,
    string? InformationRegistreCode);

/// <summary>
/// Au moins un des 5 liens doit être renseigné (contrainte CHECK en base, Prompt Maître 4.1) —
/// non revalidé côté client, le serveur reste la source de vérité (interdiction 14).
/// InformationRegistreId couvre les fonctionnalités transversales justifiées par une
/// Contrainte/Règle/Exigence non-fonctionnelle plutôt que par un Probleme ou une Entite (voir
/// docs/03-proposition-phases-05-18-v2.md).
/// </summary>
public record CreerLienTracabiliteDto(
    int? ProblemeId,
    int? FonctionnaliteId,
    int? EntiteId,
    int? CritereAcceptationId,
    int? InformationRegistreId = null);

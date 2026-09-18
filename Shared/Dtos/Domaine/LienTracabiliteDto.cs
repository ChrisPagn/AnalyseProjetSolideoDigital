namespace Shared.Dtos.Domaine;

public record LienTracabiliteDto(
    int Id,
    int? ProblemeId,
    string? ProblemeCode,
    int? FonctionnaliteId,
    string? FonctionnaliteCode,
    int? EntiteId,
    string? EntiteCode,
    int? CritereAcceptationId);

/// <summary>
/// Au moins un des 4 liens doit être renseigné (contrainte CHECK en base, Prompt Maître 4.1) —
/// non revalidé côté client, le serveur reste la source de vérité (interdiction 14).
/// </summary>
public record CreerLienTracabiliteDto(
    int? ProblemeId,
    int? FonctionnaliteId,
    int? EntiteId,
    int? CritereAcceptationId);

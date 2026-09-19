using Shared.Enums;

namespace Shared.Dtos.Domaine;

public record ActeurDto(
    int Id,
    string Code,
    int ProjetId,
    string Nom,
    string? Fonction,
    IReadOnlyList<PermissionDto> Permissions,
    SourceInformation? Source,
    StatutInformation? Statut);

/// <summary>
/// Source/Statut optionnels : la vue "Domaine analysé" (CRUD direct) ne les envoie pas et laisse
/// le Controller appliquer des valeurs par défaut ; le Mode Entretien (Phase 05) les renseigne
/// explicitement (voir InformationCompagnonService, Option B).
/// </summary>
public record UpsertActeurDto(string Nom, string? Fonction, SourceInformation? Source = null, StatutInformation? Statut = null);

public record PermissionDto(
    int Id,
    int ActeurId,
    string EntiteConcernee,
    bool PeutVoir,
    bool PeutCreer,
    bool PeutModifier,
    bool PeutSupprimer,
    bool PeutValider);

public record UpsertPermissionDto(
    string EntiteConcernee,
    bool PeutVoir,
    bool PeutCreer,
    bool PeutModifier,
    bool PeutSupprimer,
    bool PeutValider);

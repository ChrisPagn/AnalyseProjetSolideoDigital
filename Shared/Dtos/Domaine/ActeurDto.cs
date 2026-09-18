namespace Shared.Dtos.Domaine;

public record ActeurDto(
    int Id,
    string Code,
    int ProjetId,
    string Nom,
    string? Fonction,
    IReadOnlyList<PermissionDto> Permissions);

public record UpsertActeurDto(string Nom, string? Fonction);

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

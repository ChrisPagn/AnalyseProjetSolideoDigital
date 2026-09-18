namespace Shared.Dtos.Domaine;

public record EntiteDto(
    int Id,
    string Code,
    int ProjetId,
    string Nom,
    string? Description,
    string? Attributs,
    string? Relations);

public record UpsertEntiteDto(
    string Nom,
    string? Description,
    string? Attributs,
    string? Relations);

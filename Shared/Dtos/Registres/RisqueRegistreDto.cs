namespace Shared.Dtos.Registres;

public record RisqueRegistreDto(
    int Id,
    string Code,
    int ProjetId,
    string Description,
    string? Probabilite,
    string? Impact,
    string? Mesure);

public record UpsertRisqueRegistreDto(
    string Description,
    string? Probabilite,
    string? Impact,
    string? Mesure);

namespace Shared.Dtos.Registres;

public record DecisionRegistreDto(
    int Id,
    string Code,
    int ProjetId,
    string Description,
    string? Justification,
    string? AlternativesEcartees,
    DateTime Date);

public record UpsertDecisionRegistreDto(
    string Description,
    string? Justification,
    string? AlternativesEcartees);

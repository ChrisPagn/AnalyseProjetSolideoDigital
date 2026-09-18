namespace Shared.Dtos.Domaine;

public record AutomatisationDto(
    int Id,
    string Code,
    int ProjetId,
    string Declencheur,
    string? Condition,
    string Action,
    bool ValidationHumaine);

public record UpsertAutomatisationDto(
    string Declencheur,
    string? Condition,
    string Action,
    bool ValidationHumaine);

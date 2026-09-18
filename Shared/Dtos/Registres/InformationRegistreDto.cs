using Shared.Enums;

namespace Shared.Dtos.Registres;

public record InformationRegistreDto(
    int Id,
    string Code,
    int ProjetId,
    int? PhaseId,
    int? PhaseNumero,
    string Libelle,
    string? Valeur,
    SourceInformation Source,
    StatutInformation Statut);

public record UpsertInformationRegistreDto(
    int? PhaseId,
    string Libelle,
    string? Valeur,
    SourceInformation Source,
    StatutInformation Statut);

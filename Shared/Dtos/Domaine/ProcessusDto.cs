namespace Shared.Dtos.Domaine;

public record ProcessusDto(
    int Id,
    string Code,
    int ProjetId,
    string Nom,
    string? Declencheur,
    IReadOnlyList<EtapeProcessusDto> Etapes);

public record UpsertProcessusDto(string Nom, string? Declencheur);

public record EtapeProcessusDto(
    int Id,
    int ProcessusId,
    int Ordre,
    string Acteur,
    string Action,
    string? Outil,
    string? DureeEstimee,
    string? ErreursConnues,
    bool ExempleValide,
    string? ExempleDescription);

public record UpsertEtapeProcessusDto(
    int Ordre,
    string Acteur,
    string Action,
    string? Outil,
    string? DureeEstimee,
    string? ErreursConnues,
    bool ExempleValide,
    string? ExempleDescription);

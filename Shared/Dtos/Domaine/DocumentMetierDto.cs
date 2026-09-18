namespace Shared.Dtos.Domaine;

public record DocumentMetierDto(
    int Id,
    string Code,
    int ProjetId,
    string Type,
    string? Origine,
    string? Destination,
    string? Format,
    string? DureeConservation);

public record UpsertDocumentMetierDto(
    string Type,
    string? Origine,
    string? Destination,
    string? Format,
    string? DureeConservation);

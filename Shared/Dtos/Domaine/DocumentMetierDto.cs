using Shared.Enums;

namespace Shared.Dtos.Domaine;

public record DocumentMetierDto(
    int Id,
    string Code,
    int ProjetId,
    string Type,
    string? Origine,
    string? Destination,
    string? Format,
    string? DureeConservation,
    SourceInformation? Source,
    StatutInformation? Statut);

/// <summary>
/// Source/Statut optionnels : la vue "Domaine analysé" (CRUD direct) ne les envoie pas ; le Mode
/// Entretien (Phase 07) les renseigne explicitement (voir InformationCompagnonService, Option B).
/// </summary>
public record UpsertDocumentMetierDto(
    string Type,
    string? Origine,
    string? Destination,
    string? Format,
    string? DureeConservation,
    SourceInformation? Source = null,
    StatutInformation? Statut = null);

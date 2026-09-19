using Shared.Enums;

namespace Shared.Dtos.Domaine;

public record EntiteDto(
    int Id,
    string Code,
    int ProjetId,
    string Nom,
    string? Description,
    string? Attributs,
    string? Relations,
    SourceInformation? Source,
    StatutInformation? Statut);

/// <summary>
/// Source/Statut optionnels : la vue "Domaine analysé" (CRUD direct) ne les envoie pas ; le Mode
/// Entretien (Phase 06) les renseigne explicitement (voir InformationCompagnonService, Option B).
/// </summary>
public record UpsertEntiteDto(
    string Nom,
    string? Description,
    string? Attributs,
    string? Relations,
    SourceInformation? Source = null,
    StatutInformation? Statut = null);

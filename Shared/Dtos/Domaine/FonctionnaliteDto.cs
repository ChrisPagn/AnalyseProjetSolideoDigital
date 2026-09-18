using Shared.Enums;

namespace Shared.Dtos.Domaine;

public record FonctionnaliteDto(
    int Id,
    string Code,
    int ProjetId,
    int? ActeurId,
    string? ActeurNom,
    string Nom,
    string? Description,
    PrioriteMoSCoW Priorite,
    StatutFonctionnalite Statut,
    bool EstOrpheline,
    IReadOnlyList<CritereAcceptationDto> CriteresAcceptation);

public record UpsertFonctionnaliteDto(
    int? ActeurId,
    string Nom,
    string? Description,
    PrioriteMoSCoW Priorite,
    StatutFonctionnalite Statut);

public record CritereAcceptationDto(int Id, int FonctionnaliteId, string Given, string When, string Then);

public record UpsertCritereAcceptationDto(string Given, string When, string Then);

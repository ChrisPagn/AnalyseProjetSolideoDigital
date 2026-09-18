using Shared.Enums;

namespace Shared.Dtos.Projets;

/// <summary>
/// Pas de champ NiveauMaturite ici : il est exclusivement calculé par MaturiteCalculatorService
/// (Prompt Maître 4.3 et interdiction 14 — jamais assignable directement, même via l'UI).
/// </summary>
public record UpsertProjetDto(
    string Nom,
    int ClientId,
    string? StackEnvisagee,
    StatutProjet Statut);

using Shared.Enums;

namespace Shared.Dtos.Phases;

public record PhaseDto(
    int Id,
    int ProjetId,
    int Numero,
    string Nom,
    StatutPhase Statut,
    DateTime DateMaj);

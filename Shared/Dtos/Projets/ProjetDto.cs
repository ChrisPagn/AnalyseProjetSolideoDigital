using Shared.Enums;

namespace Shared.Dtos.Projets;

public record ProjetDto(
    int Id,
    string Nom,
    int ClientId,
    string ClientNom,
    DateTime DateCreation,
    NiveauMaturite NiveauMaturite,
    string? StackEnvisagee,
    StatutProjet Statut);

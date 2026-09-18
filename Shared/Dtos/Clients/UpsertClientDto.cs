namespace Shared.Dtos.Clients;

public record UpsertClientDto(
    string Nom,
    string? Secteur,
    string? Taille,
    string? Contact,
    string? Adresse,
    string? Notes);

namespace Shared.Dtos.Clients;

public record ClientDto(
    int Id,
    string Nom,
    string? Secteur,
    string? Taille,
    string? Contact,
    string? Adresse,
    string? Notes,
    int NombreProjets);

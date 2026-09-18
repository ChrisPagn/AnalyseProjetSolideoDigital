namespace Shared.Dtos.Registres;

public record HistoriqueModificationDto(
    int Id,
    string EntiteType,
    int EntiteId,
    string Champ,
    string? AncienneValeur,
    string? NouvelleValeur,
    string ModifiePar,
    DateTime DateModification);

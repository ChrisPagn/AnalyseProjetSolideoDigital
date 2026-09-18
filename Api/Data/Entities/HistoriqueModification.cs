namespace Api.Data.Entities;

/// <summary>
/// Journalisation automatique des modifications — alimentée par HistoriqueService (Prompt Maître 5.5).
/// </summary>
public class HistoriqueModification
{
    public int Id { get; set; }

    /// <summary>Nom du type d'entité modifiée, ex. "InformationRegistre".</summary>
    public required string EntiteType { get; set; }
    public int EntiteId { get; set; }
    public required string Champ { get; set; }
    public string? AncienneValeur { get; set; }
    public string? NouvelleValeur { get; set; }
    public required string ModifiePar { get; set; }
    public DateTime DateModification { get; set; }
}

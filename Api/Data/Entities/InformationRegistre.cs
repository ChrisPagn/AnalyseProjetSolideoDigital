using Shared.Enums;

namespace Api.Data.Entities;

public class InformationRegistre
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. INF-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }
    public int? PhaseId { get; set; }
    public Phase? Phase { get; set; }

    public required string Libelle { get; set; }
    public string? Valeur { get; set; }
    public SourceInformation Source { get; set; }
    public StatutInformation Statut { get; set; } = StatutInformation.Inconnu;
}

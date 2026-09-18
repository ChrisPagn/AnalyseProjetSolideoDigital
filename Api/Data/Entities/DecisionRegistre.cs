namespace Api.Data.Entities;

public class DecisionRegistre
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. DEC-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Description { get; set; }
    public string? Justification { get; set; }
    public string? AlternativesEcartees { get; set; }
    public DateTime Date { get; set; }
}

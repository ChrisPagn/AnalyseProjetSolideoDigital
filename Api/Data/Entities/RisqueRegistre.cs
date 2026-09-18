namespace Api.Data.Entities;

public class RisqueRegistre
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. R-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Description { get; set; }
    public string? Probabilite { get; set; }
    public string? Impact { get; set; }
    public string? Mesure { get; set; }
}

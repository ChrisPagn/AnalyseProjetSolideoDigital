namespace Api.Data.Entities;

public class Automatisation
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. AUTO-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Declencheur { get; set; }
    public string? Condition { get; set; }
    public required string Action { get; set; }
    public bool ValidationHumaine { get; set; }
}

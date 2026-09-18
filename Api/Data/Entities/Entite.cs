namespace Api.Data.Entities;

/// <summary>
/// Entité du domaine métier du projet analysé (pas une entité EF Core) — Prompt Maître 4.1.
/// </summary>
public class Entite
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. ENT-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Nom { get; set; }
    public string? Description { get; set; }
    public string? Attributs { get; set; }
    public string? Relations { get; set; }

    public ICollection<LienTracabilite> LiensTracabilite { get; set; } = new List<LienTracabilite>();
}

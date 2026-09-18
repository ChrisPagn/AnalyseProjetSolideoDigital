using Shared.Enums;

namespace Api.Data.Entities;

public class Probleme
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. PROB-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public required string Description { get; set; }
    public Gravite Gravite { get; set; }

    /// <summary>Occurrences par mois.</summary>
    public decimal Frequence { get; set; }
    public decimal ImpactTempsHeuresMois { get; set; }

    /// <summary>Donnée complémentaire affichée à côté du score, non intégrée au calcul (Prompt Maître 4.3).</summary>
    public decimal? CoutEstime { get; set; }

    /// <summary>= Frequence × ImpactTempsHeuresMois (Prompt Maître 4.3).</summary>
    public decimal ScoreCalcule { get; set; }

    public ICollection<LienTracabilite> LiensTracabilite { get; set; } = new List<LienTracabilite>();
}

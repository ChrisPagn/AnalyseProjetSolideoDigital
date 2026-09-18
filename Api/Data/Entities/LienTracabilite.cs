namespace Api.Data.Entities;

/// <summary>
/// Source de vérité unique pour la couverture besoin ↔ fonctionnalité (Prompt Maître 4.1, 4.3).
/// Liens optionnels, mais au moins un requis — imposé en base par une contrainte CHECK
/// (voir AnalyseProjetDbContext.OnModelCreating) plutôt que par la seule validation applicative,
/// pour empêcher toute ligne orpheline même en cas de bug d'un futur appelant.
/// </summary>
public class LienTracabilite
{
    public int Id { get; set; }

    public int? ProblemeId { get; set; }
    public Probleme? Probleme { get; set; }

    public int? FonctionnaliteId { get; set; }
    public Fonctionnalite? Fonctionnalite { get; set; }

    public int? EntiteId { get; set; }
    public Entite? Entite { get; set; }

    public int? CritereAcceptationId { get; set; }
    public CritereAcceptation? CritereAcceptation { get; set; }
}

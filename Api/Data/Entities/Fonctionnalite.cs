using Shared.Enums;

namespace Api.Data.Entities;

public class Fonctionnalite
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. F-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    public int? ActeurId { get; set; }
    public Acteur? Acteur { get; set; }

    public required string Nom { get; set; }
    public string? Description { get; set; }
    public PrioriteMoSCoW Priorite { get; set; }
    public StatutFonctionnalite Statut { get; set; } = StatutFonctionnalite.Identifiee;

    /// <summary>
    /// PAS de champ BesoinCouvert en texte libre ici (interdiction absolue, Prompt Maître 14) :
    /// la couverture d'un besoin se lit exclusivement via LienTracabilite.
    /// </summary>
    public ICollection<LienTracabilite> LiensTracabilite { get; set; } = new List<LienTracabilite>();
    public ICollection<CritereAcceptation> CriteresAcceptation { get; set; } = new List<CritereAcceptation>();
}

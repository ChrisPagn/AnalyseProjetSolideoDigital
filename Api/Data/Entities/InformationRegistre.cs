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

    /// <summary>
    /// Lien polymorphe optionnel vers une entité du domaine analysé (Acteur, Entite,
    /// DocumentMetier, Fonctionnalite, Automatisation) — porte Source/Statut pour ces entités
    /// créées en Phases 05-09, qui n'ont pas ces colonnes elles-mêmes (Option B, voir
    /// docs/03-proposition-phases-05-18-v2.md). Les deux champs sont renseignés ensemble ou pas
    /// du tout (contrainte CHECK, voir AnalyseProjetDbContext).
    /// </summary>
    public TypeEntiteDomaine? EntiteType { get; set; }
    public int? EntiteReferenceId { get; set; }
}

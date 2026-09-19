using Shared.Enums;

namespace Api.Data.Entities;

public class Projet
{
    public int Id { get; set; }
    public required string Nom { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime DateCreation { get; set; }

    /// <summary>
    /// Calculé par MaturiteCalculatorService — jamais assigné directement par un composant Blazor
    /// ou un Controller (voir Prompt Maître 4.3 et 14, règle non contournable).
    /// </summary>
    public NiveauMaturite NiveauMaturite { get; set; } = NiveauMaturite.Niveau0Inconnu;

    public string? StackEnvisagee { get; set; }
    public StatutProjet Statut { get; set; } = StatutProjet.EnCours;

    /// <summary>
    /// Notes libres de préparation avant le premier RDV (secteur, concurrents connus, outils
    /// actuels déjà repérés) — hors numérotation des 18 Phases, aucun impact sur la maturité ni
    /// sur les registres (voir docs/03-proposition-phases-05-18-v2.md, section "Notes de
    /// préparation").
    /// </summary>
    public string? NotesPreparation { get; set; }

    public ICollection<Phase> Phases { get; set; } = new List<Phase>();
    public ICollection<InformationRegistre> Informations { get; set; } = new List<InformationRegistre>();
    public ICollection<QuestionRegistre> Questions { get; set; } = new List<QuestionRegistre>();
    public ICollection<RisqueRegistre> Risques { get; set; } = new List<RisqueRegistre>();
    public ICollection<DecisionRegistre> Decisions { get; set; } = new List<DecisionRegistre>();
    public ICollection<Probleme> Problemes { get; set; } = new List<Probleme>();
    public ICollection<Processus> Processus { get; set; } = new List<Processus>();
    public ICollection<Acteur> Acteurs { get; set; } = new List<Acteur>();
    public ICollection<Entite> Entites { get; set; } = new List<Entite>();
    public ICollection<DocumentMetier> Documents { get; set; } = new List<DocumentMetier>();
    public ICollection<Fonctionnalite> Fonctionnalites { get; set; } = new List<Fonctionnalite>();
    public ICollection<Automatisation> Automatisations { get; set; } = new List<Automatisation>();
}

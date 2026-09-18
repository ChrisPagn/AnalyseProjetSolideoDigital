using Shared.Enums;

namespace Api.Data.Entities;

/// <summary>
/// Une des 18 phases du protocole d'analyse — voir Guide des 18 phases.
/// </summary>
public class Phase
{
    public int Id { get; set; }
    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }

    /// <summary>Numéro de la phase, 1 à 18.</summary>
    public int Numero { get; set; }
    public required string Nom { get; set; }
    public StatutPhase Statut { get; set; } = StatutPhase.NonCommencee;
    public DateTime DateMaj { get; set; }
}

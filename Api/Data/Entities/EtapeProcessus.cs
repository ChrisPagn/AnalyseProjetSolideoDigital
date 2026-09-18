namespace Api.Data.Entities;

public class EtapeProcessus
{
    public int Id { get; set; }
    public int ProcessusId { get; set; }
    public Processus? Processus { get; set; }

    public int Ordre { get; set; }
    public required string Acteur { get; set; }
    public required string Action { get; set; }
    public string? Outil { get; set; }
    public string? DureeEstimee { get; set; }
    public string? ErreursConnues { get; set; }

    /// <summary>
    /// true seulement si confirmée par un exemple concret vérifié (Guide 18 phases, Phase 03).
    /// Tant que false, l'étape reste "À confirmer" — jamais "Validée" — même si la phase est marquée Terminée
    /// (incohérence détectée par ContradictionDetectorService, Prompt Maître 4.3).
    /// </summary>
    public bool ExempleValide { get; set; }
    public string? ExempleDescription { get; set; }
}

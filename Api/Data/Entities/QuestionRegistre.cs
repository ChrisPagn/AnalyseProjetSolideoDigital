using Shared.Enums;

namespace Api.Data.Entities;

public class QuestionRegistre
{
    public int Id { get; set; }

    /// <summary>Code auto-incrémenté par projet, ex. Q-001.</summary>
    public required string Code { get; set; }

    public int ProjetId { get; set; }
    public Projet? Projet { get; set; }
    public int? PhaseId { get; set; }
    public Phase? Phase { get; set; }

    public required string Question { get; set; }
    public ImportanceQuestion Importance { get; set; } = ImportanceQuestion.Normale;
    public StatutQuestion Statut { get; set; } = StatutQuestion.Ouverte;
}

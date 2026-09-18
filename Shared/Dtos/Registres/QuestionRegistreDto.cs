using Shared.Enums;

namespace Shared.Dtos.Registres;

public record QuestionRegistreDto(
    int Id,
    string Code,
    int ProjetId,
    int? PhaseId,
    int? PhaseNumero,
    string Question,
    ImportanceQuestion Importance,
    StatutQuestion Statut);

public record UpsertQuestionRegistreDto(
    int? PhaseId,
    string Question,
    ImportanceQuestion Importance,
    StatutQuestion Statut);

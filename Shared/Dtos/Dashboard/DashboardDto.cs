using Shared.Enums;

namespace Shared.Dtos.Dashboard;

/// <summary>
/// Vue d'ensemble multi-projets (Prompt Maître, section 10 étape N) : une carte par projet avec
/// sa jauge de maturité, plus le registre global des questions bloquantes ouvertes tous projets
/// confondus.
/// </summary>
public record DashboardDto(
    IReadOnlyList<ProjetResumeDto> Projets,
    IReadOnlyList<QuestionBloquanteDto> QuestionsBloquantes);

public record ProjetResumeDto(
    int Id,
    string Nom,
    string ClientNom,
    NiveauMaturite NiveauMaturite,
    StatutProjet Statut,
    int NombrePhasesTerminees,
    int NombreQuestionsBloquantesOuvertes);

public record QuestionBloquanteDto(
    int ProjetId,
    string ProjetNom,
    string Code,
    string Question);

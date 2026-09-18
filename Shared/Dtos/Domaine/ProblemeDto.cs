using Shared.Enums;

namespace Shared.Dtos.Domaine;

public record ProblemeDto(
    int Id,
    string Code,
    int ProjetId,
    string Description,
    Gravite Gravite,
    decimal Frequence,
    decimal ImpactTempsHeuresMois,
    decimal? CoutEstime,
    decimal ScoreCalcule,
    bool EstCouvert);

/// <summary>
/// Pas de champ ScoreCalcule ici : il est exclusivement calculé côté serveur
/// (= Frequence × ImpactTempsHeuresMois, Prompt Maître 4.3), jamais saisi directement.
/// </summary>
public record UpsertProblemeDto(
    string Description,
    Gravite Gravite,
    decimal Frequence,
    decimal ImpactTempsHeuresMois,
    decimal? CoutEstime);

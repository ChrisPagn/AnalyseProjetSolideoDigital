namespace Shared.Enums;

/// <summary>
/// Niveau de maturité du projet (0 à 5) — voir légende unifiée, Prompt Maître section 4.0
/// et règles de calcul, section 4.3.
/// </summary>
public enum NiveauMaturite
{
    Niveau0Inconnu = 0,
    Niveau1Comprehension = 1,
    Niveau2AnalyseMetier = 2,
    Niveau3Specification = 3,
    Niveau4Conception = 4,
    Niveau5PretPourDev = 5
}

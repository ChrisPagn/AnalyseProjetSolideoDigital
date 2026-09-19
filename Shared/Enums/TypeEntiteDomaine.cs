namespace Shared.Enums;

/// <summary>
/// Discriminant du lien polymorphe InformationRegistre → entité du domaine analysé (Acteur,
/// Entite, DocumentMetier, Fonctionnalite, Automatisation) — Option B retenue pour porter
/// Source/Statut sur les Phases 05-09 sans dupliquer ces deux colonnes sur 5 tables (voir
/// docs/03-proposition-phases-05-18-v2.md, section "Impact technique résumé").
/// </summary>
public enum TypeEntiteDomaine
{
    Acteur,
    Entite,
    DocumentMetier,
    Fonctionnalite,
    Automatisation
}

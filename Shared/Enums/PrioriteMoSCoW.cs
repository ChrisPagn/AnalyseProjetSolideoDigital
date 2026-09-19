namespace Shared.Enums;

public enum PrioriteMoSCoW
{
    /// <summary>
    /// Valeur par défaut à la création d'une Fonctionnalite en Phase 08 — la Priorite n'est
    /// définitivement tranchée qu'en Phase 14 (Priorisation MVP), pour éviter la double saisie
    /// et le risque qu'une valeur choisie à la volée en Phase 08 ne soit jamais revue (voir
    /// docs/03-proposition-phases-05-18-v2.md).
    /// </summary>
    NonArbitree,
    MustHave,
    ShouldHave,
    CouldHave,
    WontHave
}
